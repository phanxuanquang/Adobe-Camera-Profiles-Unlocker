using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public SettingsViewModel(IUpdateService updates, ILauncher launcher, IInstalledStore store)
    {
        ProfileFolders = store.Folders;
        _updates = updates;
        _launcher = launcher;
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

    [RelayCommand]
    private async Task CheckForUpdatesAsync(CancellationToken ct)
    {
        UpdateStatus = "Checking…";
        LatestRelease = await _updates.GetNewerAsync(AppVersion, ct);
        UpdateStatus = LatestRelease is { } r ? $"Version {r.Version} is available." : "You're up to date.";
    }

    [RelayCommand]
    private async Task DownloadAsync(CancellationToken ct)
    {
        if (LatestRelease is { } r) await _launcher.OpenUrlAsync(r.DownloadUrl, ct);
    }

    [RelayCommand]
    private Task OpenFolderAsync(string path, CancellationToken ct) => _launcher.OpenFolderAsync(path, ct);
}
