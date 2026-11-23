using System.Text.Json;
using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Infrastructure.Platform;
using Unlocker.Infrastructure.Serialization;

namespace Unlocker.Infrastructure.Storage;

internal sealed class JsonSettings : ISettings
{
    private readonly AppPaths _paths;

    public JsonSettings(AppPaths paths, ILogger<JsonSettings> logger)
    {
        _paths = paths;
        if (!File.Exists(paths.SettingsFile)) return;
        try
        {
            var data = JsonSerializer.Deserialize(File.ReadAllText(paths.SettingsFile), UnlockerJsonContext.Default.SettingsData);
            TargetCamera = data?.TargetCamera;
        }
        catch (JsonException e)
        {
            // Corrupt file: start fresh. The next save overwrites it.
            logger.LogWarning(e, "Corrupt settings file {File}, using defaults", paths.SettingsFile);
        }
    }

    public string? TargetCamera { get; set; }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        Directory.CreateDirectory(_paths.AppDataDir);
        await using var stream = File.Create(_paths.SettingsFile);
        await JsonSerializer.SerializeAsync(stream, new SettingsData(TargetCamera), UnlockerJsonContext.Default.SettingsData, ct);
    }
}
