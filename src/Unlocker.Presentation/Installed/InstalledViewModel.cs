using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Common;
using Unlocker.Presentation.Ports;

namespace Unlocker.Presentation.Installed;

public sealed partial class InstalledViewModel : ObservableObject
{
    private readonly IInstalledStore _store;
    private readonly IDialogService _dialogs;
    private readonly ILauncher _launcher;
    private readonly ILogger<InstalledViewModel> _logger;

    public InstalledViewModel(IInstalledStore store, IDialogService dialogs, ILauncher launcher, ILogger<InstalledViewModel> logger)
    {
        _logger = logger;
        _store = store;
        _dialogs = dialogs;
        _launcher = launcher;
        _store.Changed += (_, _) => Refresh();
        Refresh();
    }

    /// <summary>One group per target camera.</summary>
    public ObservableCollection<InstalledGroup> Groups { get; } = [];

    public bool IsEmpty => Groups.Count == 0;

    [ObservableProperty]
    public partial StatusMessage? Status { get; set; }

    private void Refresh()
    {
        Groups.Clear();
        foreach (var g in _store.All.GroupBy(i => i.TargetModel).OrderBy(g => g.Key))
            Groups.Add(new InstalledGroup(g.Key, g.OrderBy(i => i.Name).ToList()));
        OnPropertyChanged(nameof(IsEmpty));
        RemoveAllCommand.NotifyCanExecuteChanged();
    }

    /// <summary>A single item is removed without asking: Undo covers it.</summary>
    [RelayCommand]
    private Task RemoveAsync(InstalledCameraProfile item) => RemoveWithUndoAsync([item]);

    [RelayCommand]
    private async Task RemoveGroupAsync(InstalledGroup group)
    {
        if (await ConfirmAsync(group.Items, $"Remove camera profiles for {group.TargetModel}?")) await RemoveWithUndoAsync(group.Items);
    }

    [RelayCommand(CanExecute = nameof(HasItems))]
    private async Task RemoveAllAsync()
    {
        var all = _store.All.ToList();
        if (await ConfirmAsync(all, "Remove all installed camera profiles?")) await RemoveWithUndoAsync(all);
    }

    private bool HasItems() => Groups.Count > 0;

    private Task<bool> ConfirmAsync(IReadOnlyList<InstalledCameraProfile> list, string title) => _dialogs.ConfirmAsync(title,
        $"{Text.Profiles(list.Count)} will be deleted from the profile folders. You can undo right after.\n\n"
        + string.Join("\n", list.Select(p => $"{p.Name} for {p.TargetModel}")),
        $"Remove {list.Count}");

    private async Task RemoveWithUndoAsync(IReadOnlyList<InstalledCameraProfile> list)
    {
        try
        {
            await _store.RemoveAsync(list);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Removing {Count} camera profiles failed", list.Count);
            Status = new StatusMessage(StatusSeverity.Error, "Couldn't remove camera profiles.", ex.Message);
            return;
        }
        var title = list.Count == 1 ? $"Removed {list[0].Name} for {list[0].TargetModel}." : $"Removed {Text.Profiles(list.Count)}.";
        IReadOnlyList<StatusAction> actions = [new StatusAction("Undo", new AsyncRelayCommand(() => RestoreAsync(list)))];
        Status = new StatusMessage(StatusSeverity.Info, title, "Restart Lightroom or Photoshop to apply.", actions);
    }

    private async Task RestoreAsync(IReadOnlyList<InstalledCameraProfile> list)
    {
        try
        {
            await _store.RestoreAsync(list);
            Status = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Restoring {Count} camera profiles failed", list.Count);
            Status = new StatusMessage(StatusSeverity.Error, "Couldn't undo the removal.", ex.Message);
        }
    }

    [RelayCommand]
    private Task OpenFolderAsync(CancellationToken ct) => _launcher.OpenFolderAsync(_store.Folders[0], ct);
}
