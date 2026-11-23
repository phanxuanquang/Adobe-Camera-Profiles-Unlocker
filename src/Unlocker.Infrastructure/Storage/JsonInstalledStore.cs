using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Infrastructure.Platform;
using Unlocker.Infrastructure.Serialization;

namespace Unlocker.Infrastructure.Storage;

/// <summary>Manifest in installed.json plus a session trash for Undo. Disposing empties the trash.</summary>
internal sealed class JsonInstalledStore(AppPaths paths, ILogger<JsonInstalledStore> logger) : IInstalledStore, IAsyncDisposable
{
    private const int ErrorCancelled = 1223;

    private List<InstalledCameraProfile> _items = [];

    /// <summary>Original path to trash path.</summary>
    private readonly Dictionary<string, string> _trash = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<InstalledCameraProfile> All => _items;

    public IReadOnlyList<string> Folders => [paths.DcpOutputDir, paths.XmpOutputDir];

    public event EventHandler? Changed;

    public async Task LoadAsync(CancellationToken ct = default)
    {
        List<InstalledCameraProfile> items = [];
        if (File.Exists(paths.ManifestFile))
        {
            try
            {
                await using var stream = File.OpenRead(paths.ManifestFile);
                items = await JsonSerializer.DeserializeAsync(stream, UnlockerJsonContext.Default.ListInstalledCameraProfile, ct) ?? [];
            }
            catch (JsonException e)
            {
                // Corrupt manifest: start empty. Files stay on disk because untracked files are never deleted.
                logger.LogWarning(e, "Corrupt manifest {File}, starting empty", paths.ManifestFile);
            }
        }
        // Drop entries the user deleted by hand. Legacy entries are always re-detected from disk.
        items.RemoveAll(p => p.IsLegacy || !p.Files.Any(File.Exists));
        items.AddRange(DetectLegacy(items));
        _items = items;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>The old admin app wrote "{name} (for {target}).dcp" straight into the CameraProfiles root.</summary>
    private IEnumerable<InstalledCameraProfile> DetectLegacy(List<InstalledCameraProfile> known)
    {
        if (!Directory.Exists(paths.LegacyDcpDir)) yield break;
        var tracked = known.SelectMany(p => p.Files).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(paths.LegacyDcpDir, "* (for *).dcp"))
        {
            if (tracked.Contains(file)) continue;
            var stem = Path.GetFileNameWithoutExtension(file);
            var at = stem.LastIndexOf(" (for ", StringComparison.Ordinal);
            if (at <= 0 || !stem.EndsWith(')')) continue;
            var target = stem[(at + " (for ".Length)..^1];
            yield return new InstalledCameraProfile(stem[..at], "Unknown", target, [file], IsLegacy: true);
        }
    }

    public async Task AddAsync(InstalledCameraProfile item, CancellationToken ct = default)
    {
        _items.RemoveAll(p => p.Name == item.Name && p.TargetModel == item.TargetModel && !p.IsLegacy);
        _items.Add(item);
        await SaveAsync(ct);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveAsync(IEnumerable<InstalledCameraProfile> items, CancellationToken ct = default)
    {
        var list = items.ToList();
        var legacy = list.Where(p => p.IsLegacy).ToList();
        if (legacy.Count > 0) await DeleteElevatedAsync(legacy.SelectMany(p => p.Files).Where(File.Exists).ToList(), ct);

        foreach (var item in list.Where(p => !p.IsLegacy))
        {
            foreach (var file in item.Files.Where(File.Exists))
            {
                Directory.CreateDirectory(paths.TrashDir);
                var trashed = Path.Combine(paths.TrashDir, $"{Guid.NewGuid():N}{Path.GetExtension(file)}");
                File.Move(file, trashed);
                _trash[file] = trashed;
            }
        }
        _items.RemoveAll(list.Contains);
        await SaveAsync(ct);
        logger.LogInformation("Removed {Count} installed profiles ({LegacyCount} legacy)", list.Count, legacy.Count);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task RestoreAsync(IEnumerable<InstalledCameraProfile> items, CancellationToken ct = default)
    {
        var restored = items.Where(p => !p.IsLegacy).ToList();
        foreach (var item in restored)
        {
            foreach (var file in item.Files)
            {
                if (!_trash.Remove(file, out var trashed) || !File.Exists(trashed)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.Move(trashed, file, overwrite: true);
            }
            if (!_items.Contains(item)) _items.Add(item);
        }
        await SaveAsync(ct);
        logger.LogInformation("Restored {Count} installed profiles", restored.Count);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Legacy files are admin-owned. This is the only elevation in the app. UAC "No" throws OperationCanceledException.</summary>
    private async Task DeleteElevatedAsync(IReadOnlyList<string> files, CancellationToken ct)
    {
        if (files.Count == 0) return;
        // ponytail: paths come from our own "* (for *).dcp" scan and are quoted; a '%' in a name could still expand in cmd.
        var psi = new ProcessStartInfo("cmd.exe", "/c del /f /q " + string.Join(' ', files.Select(f => $"\"{f}\"")))
        {
            Verb = "runas",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };
        try
        {
            using var process = Process.Start(psi);
            if (process is not null) await process.WaitForExitAsync(ct);
        }
        catch (Win32Exception e) when (e.NativeErrorCode == ErrorCancelled)
        {
            logger.LogInformation("Elevation cancelled by user, {Count} legacy files kept", files.Count);
            throw new OperationCanceledException("Elevation cancelled.", e);
        }
        var left = files.Where(File.Exists).ToList();
        if (left.Count > 0)
        {
            logger.LogWarning("Elevated delete left {Count} legacy files: {Files}", left.Count, string.Join("; ", left));
            throw new IOException("Some legacy files could not be deleted.");
        }
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(paths.AppDataDir);
        await using var stream = File.Create(paths.ManifestFile);
        await JsonSerializer.SerializeAsync(stream, _items, UnlockerJsonContext.Default.ListInstalledCameraProfile, ct);
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(paths.TrashDir)) Directory.Delete(paths.TrashDir, recursive: true);
        return ValueTask.CompletedTask;
    }
}
