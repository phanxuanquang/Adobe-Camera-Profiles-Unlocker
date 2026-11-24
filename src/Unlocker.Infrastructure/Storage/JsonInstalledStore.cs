using Microsoft.Extensions.Logging;
using System.Text.Json;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Infrastructure.Platform;
using Unlocker.Infrastructure.Serialization;

namespace Unlocker.Infrastructure.Storage;

/// <summary>Manifest in installed.json plus a session trash for Undo. Disposing empties the trash.</summary>
internal sealed class JsonInstalledStore(AppPaths paths, ILogger<JsonInstalledStore> logger) : IInstalledStore, IDisposable
{
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
            catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
            {
                // Corrupt or unreadable manifest: start empty. Files stay on disk because untracked files are never deleted.
                logger.LogWarning(e, "Corrupt or unreadable manifest {File}, starting empty", paths.ManifestFile);
            }
        }
        // Hand-edited manifest: drop null or incomplete entries instead of crashing at startup.
        var broken = items.RemoveAll(p => p?.Name is null || p.SourceModel is null || p.TargetModel is null || p.Files is null);
        if (broken > 0) logger.LogWarning("Skipped {Count} invalid entries in manifest {File}", broken, paths.ManifestFile);
        // Drop entries the user deleted by hand.
        items.RemoveAll(p => !p.Files.Any(File.Exists));
        _items = items;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task AddAsync(InstalledCameraProfile item, CancellationToken ct = default)
    {
        _items.RemoveAll(p => SameKey(p, item));
        _items.Add(item);
        await SaveAsync(ct);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static bool SameKey(InstalledCameraProfile a, InstalledCameraProfile b)
        => a.Name == b.Name && a.SourceModel == b.SourceModel && a.TargetModel == b.TargetModel;

    public async Task RemoveAsync(IEnumerable<InstalledCameraProfile> items)
    {
        var list = items.ToList();
        foreach (var item in list)
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
        await SaveAsync(CancellationToken.None);
        logger.LogInformation("Removed {Count} installed profiles", list.Count);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task RestoreAsync(IEnumerable<InstalledCameraProfile> items)
    {
        var restored = new List<InstalledCameraProfile>();
        foreach (var item in items)
        {
            // Reinstalled since removal, or a file now sits at the path: keep the newer one, leave ours in trash.
            if (_items.Any(p => SameKey(p, item)) || item.Files.Any(f => _trash.ContainsKey(f) && File.Exists(f)))
            {
                logger.LogWarning("Not restoring {Name} for {Target}: already installed or file exists", item.Name, item.TargetModel);
                continue;
            }
            foreach (var file in item.Files)
            {
                if (!_trash.Remove(file, out var trashed) || !File.Exists(trashed)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.Move(trashed, file);
            }
            _items.Add(item);
            restored.Add(item);
        }
        await SaveAsync(CancellationToken.None);
        logger.LogInformation("Restored {Count} installed profiles", restored.Count);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(paths.AppDataDir);
        // Write a temp file, then rename over the manifest: a crash or cancel never leaves a half-written manifest.
        var tmp = paths.ManifestFile + ".tmp";
        try
        {
            await using (var stream = File.Create(tmp))
                await JsonSerializer.SerializeAsync(stream, _items, UnlockerJsonContext.Default.ListInstalledCameraProfile, ct);
            File.Move(tmp, paths.ManifestFile, overwrite: true);
        }
        catch
        {
            File.Delete(tmp);
            throw;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(paths.TrashDir)) Directory.Delete(paths.TrashDir, recursive: true);
    }
}
