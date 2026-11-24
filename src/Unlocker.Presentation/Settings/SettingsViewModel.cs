using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Ports;

namespace Unlocker.Presentation.Settings;

public sealed partial class SettingsViewModel : ObservableObject
{
    public static readonly Uri GitHubUrl = new("https://github.com/phanxuanquang/Adobe-Camera-Profiles-Unlocker");
    public static readonly Uri DonateUrl = new("https://i.imgur.com/XYXfisQ.jpeg");

    private readonly IUpdateService _updates;
    private readonly ILauncher _launcher;
    private readonly ILogger<SettingsViewModel> _logger;

    public SettingsViewModel(IUpdateService updates, ILauncher launcher, IInstalledStore store, ILogger<SettingsViewModel> logger)
    {
        ProfileFolders = store.Folders;
        _updates = updates;
        _launcher = launcher;
        _logger = logger;
    }

    /// <summary>DCP folder, then XMP folder.</summary>
    public IReadOnlyList<string> ProfileFolders { get; }

    public Version AppVersion { get; } = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(3, 0, 0);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
    public partial ReleaseInfo? LatestRelease { get; set; }

    public bool IsUpdateAvailable => LatestRelease is not null;

    [ObservableProperty]
    public partial string UpdateStatus { get; set; } = "";

    /// <summary>The Settings page runs this each time it opens.</summary>
    [RelayCommand]
    private async Task CheckForUpdatesAsync(CancellationToken ct)
    {
        UpdateStatus = "Checking for updates…";
        try
        {
            LatestRelease = await _updates.GetNewerAsync(AppVersion, ct);
            UpdateStatus = LatestRelease is { } r ? $"Version {r.Version} is available." : "You're up to date.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            // Any failure (offline, GitHub down, bad response, timeout) only means "unknown": say so, never "up to date".
            _logger.LogWarning(ex, "Update check failed");
            LatestRelease = null;
            UpdateStatus = "Couldn't check for updates. Check your internet connection.";
        }
    }

    [RelayCommand]
    private async Task OpenDownloadPageAsync(CancellationToken ct)
    {
        if (LatestRelease is { } r) await _launcher.OpenUrlAsync(r.PageUrl, ct);
    }

    [RelayCommand]
    private Task OpenFolderAsync(string path, CancellationToken ct) => _launcher.OpenFolderAsync(path, ct);
}
