using CommunityToolkit.Mvvm.ComponentModel;
using Unlocker.Core.Abstractions;

namespace Unlocker.Presentation.Shell;

public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IInstalledStore _store;

    public ShellViewModel(IInstalledStore store)
    {
        _store = store;
        _store.Changed += (_, _) => InstalledCount = _store.All.Count;
    }

    [ObservableProperty]
    public partial int InstalledCount { get; set; }

    /// <summary>Call once after the window shows.</summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _store.LoadAsync(ct);
        InstalledCount = _store.All.Count;
    }
}
