using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Ports;

namespace Unlocker.Presentation.Startup;

public sealed partial class StartupViewModel(IEnvironmentProbe probe, ICameraCatalog catalog, ILauncher launcher) : ObservableObject
{
    public static readonly Uri CameraRawDownload = new("https://www.adobe.com/go/acr_installer_win");

    [ObservableProperty]
    public partial StartupState State { get; set; } = StartupState.Loading;

    /// <summary>Non-blocking issues, shown as a warning on the Unlock page.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<EnvironmentIssue> Warnings { get; set; } = [];

    /// <summary>Also "Try again". Warms the catalog cache for the Unlock page.</summary>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        State = StartupState.Loading;
        var issues = probe.Check();
        if (issues.Contains(EnvironmentIssue.CameraRawMissing))
        {
            State = StartupState.CameraRawMissing;
            return;
        }
        await catalog.LoadAsync(ct);
        Warnings = issues;
        State = StartupState.Ready;
    }

    [RelayCommand]
    private Task GetCameraRawAsync(CancellationToken ct) => launcher.OpenUrlAsync(CameraRawDownload, ct);
}
