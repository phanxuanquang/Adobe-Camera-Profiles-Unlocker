using CommunityToolkit.Mvvm.ComponentModel;
using Unlocker.Core.Abstractions;
using Unlocker.Presentation.Settings;

namespace Unlocker.Presentation.Shell;

public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IInstalledStore _store;

    public ShellViewModel(IInstalledStore store, SettingsViewModel settings)
    {
        _store = store;
        Settings = settings;
        _store.Changed += (_, _) => InstalledCount = _store.All.Count;
    }

    /// <summary>Owns the update state shown as a dot on the Settings nav item.</summary>
    public SettingsViewModel Settings { get; }

    [ObservableProperty]
    public partial int InstalledCount { get; set; }

    /// <summary>Call once after the window shows. The update check runs in the background.</summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _store.LoadAsync(ct);
        InstalledCount = _store.All.Count;
        _ = Settings.CheckForUpdatesCommand.ExecuteAsync(null);
    }
}
