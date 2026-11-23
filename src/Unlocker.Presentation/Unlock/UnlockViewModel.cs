using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Common;

namespace Unlocker.Presentation.Unlock;

/// <summary>Singleton so the selection survives tab switches.</summary>
public sealed partial class UnlockViewModel : ObservableObject
{
    private const int MaxChips = 4;

    private readonly ICameraCatalog _catalog;
    private readonly ICameraProfileInstaller _installer;
    private readonly IInstalledStore _store;
    private readonly ISettings _settings;
    private readonly ILogger<UnlockViewModel> _logger;

    /// <summary>Tiles per brand for its current source model. Reusing instances keeps IsSelected and focus across filters.</summary>
    private readonly Dictionary<BrandItemViewModel, List<CameraProfileItemViewModel>> _items = [];

    public UnlockViewModel(ICameraCatalog catalog, ICameraProfileInstaller installer, IInstalledStore store, ISettings settings, ILogger<UnlockViewModel> logger)
    {
        _catalog = catalog;
        _installer = installer;
        _store = store;
        _settings = settings;
        _logger = logger;
        TargetCamera = settings.TargetCamera;
        _store.Changed += (_, _) => RefreshInstalledFlags();
        Selected.CollectionChanged += (_, _) => OnSelectionChanged();
    }

    /// <summary>"View installed" in the success message. The shell navigates.</summary>
    public event EventHandler? ViewInstalledRequested;

    public ObservableCollection<BrandItemViewModel> Brands { get; } = [];

    /// <summary>Brands shown in the list: all of them, or only those with matches while searching.</summary>
    public ObservableCollection<BrandItemViewModel> VisibleBrands { get; } = [];

    /// <summary>Camera profiles of the selected brand's source model.</summary>
    public ObservableCollection<CameraProfileItemViewModel> Profiles { get; } = [];

    public ObservableCollection<CameraProfileGroup> SearchResults { get; } = [];

    /// <summary>Tray content, in selection order.</summary>
    public ObservableCollection<CameraProfileItemViewModel> Selected { get; } = [];

    public IReadOnlyList<Brand> Catalog { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFirstRun))]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    public partial string? TargetCamera { get; set; }

    public bool IsFirstRun => TargetCamera is null;

    [ObservableProperty]
    public partial BrandItemViewModel? SelectedBrand { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearching))]
    public partial string SearchQuery { get; set; } = "";

    public bool IsSearching => Query.Length > 0;

    private string Query => SearchQuery.Trim();

    [ObservableProperty]
    public partial bool IsSearchEmpty { get; set; }

    [ObservableProperty]
    public partial string ProfilesTitle { get; set; } = "";

    [ObservableProperty]
    public partial string ToggleAllLabel { get; set; } = "";

    [ObservableProperty]
    public partial string InstallLabel { get; set; } = "Install";

    [ObservableProperty]
    public partial IReadOnlyList<CameraProfileItemViewModel> TrayChips { get; set; } = [];

    /// <summary>"+N more" after the chips, or null.</summary>
    [ObservableProperty]
    public partial string? MoreText { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ProgressText { get; set; }

    [ObservableProperty]
    public partial StatusMessage? Status { get; set; }

    /// <summary>Idempotent: the view calls it on every navigation.</summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (Brands.Count > 0) return;
        Catalog = await _catalog.LoadAsync(ct);
        foreach (var brand in Catalog)
        {
            var item = new BrandItemViewModel(brand);
            item.PropertyChanged += OnBrandChanged;
            Brands.Add(item);
        }
        MarkYourBrand();
        SelectedBrand = Brands.FirstOrDefault(b => !b.IsYourBrand) ?? Brands.FirstOrDefault();
        RebuildProfiles();
    }

    partial void OnSelectedBrandChanged(BrandItemViewModel? value) => RebuildProfiles();

    // No debounce: a few hundred models, all in memory.
    partial void OnSearchQueryChanged(string value) => RebuildProfiles();

    private void MarkYourBrand()
    {
        foreach (var b in Brands)
            b.IsYourBrand = b.Brand.Models.Any(m => string.Equals(m.Name, TargetCamera, StringComparison.OrdinalIgnoreCase));
    }

    private List<CameraProfileItemViewModel> ItemsOf(BrandItemViewModel brand)
    {
        if (_items.TryGetValue(brand, out var list)) return list;
        var installed = InstalledNames();
        list = brand.SourceModel.Profiles.Select(p =>
        {
            var item = new CameraProfileItemViewModel(brand.Brand, brand.SourceModel, p) { IsInstalled = installed.Contains(p.Name) };
            item.PropertyChanged += OnProfileChanged;
            return item;
        }).ToList();
        _items[brand] = list;
        return list;
    }

    private List<CameraProfileItemViewModel> Matches(BrandItemViewModel brand) => Query is { Length: > 0 } q
        ? ItemsOf(brand).Where(i => $"{i.Name} {brand.Name} {brand.Family}".Contains(q, StringComparison.OrdinalIgnoreCase)).ToList()
        : ItemsOf(brand);

    /// <summary>Runs on brand, query or source model change only. Never on a checkbox toggle, so focus stays put.</summary>
    private void RebuildProfiles()
    {
        Profiles.Clear();
        SearchResults.Clear();
        if (IsSearching)
        {
            foreach (var b in Brands)
                if (Matches(b) is { Count: > 0 } m) SearchResults.Add(new CameraProfileGroup($"{b.Name} {b.Family}", m));
        }
        else if (SelectedBrand is { } brand)
        {
            foreach (var item in ItemsOf(brand)) Profiles.Add(item);
        }
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        foreach (var b in Brands)
        {
            var m = Matches(b);
            b.VisibleCount = m.Count;
            b.SelectedCount = m.Count(i => i.IsSelected);
        }
        SyncVisibleBrands();
        var total = SearchResults.Sum(g => g.Items.Count);
        IsSearchEmpty = IsSearching && total == 0;
        ProfilesTitle = !IsSearching ? (SelectedBrand is { } s ? $"{s.Name} {s.Family}" : "")
            : total > 0 ? $"{Text.Profiles(total)} match \"{Query}\"" : $"No camera profiles match \"{Query}\"";
        ToggleAllLabel = Profiles.All(p => p.IsSelected) ? "Clear choices" : "Select all";
    }

    /// <summary>Removes and inserts in place instead of resetting, so the list keeps its selection and scroll.</summary>
    private void SyncVisibleBrands()
    {
        var visible = Brands.Where(b => !IsSearching || b.VisibleCount > 0).ToList();
        for (var i = VisibleBrands.Count - 1; i >= 0; i--)
            if (!visible.Contains(VisibleBrands[i])) VisibleBrands.RemoveAt(i);
        for (var i = 0; i < visible.Count; i++)
            if (i >= VisibleBrands.Count || VisibleBrands[i] != visible[i]) VisibleBrands.Insert(i, visible[i]);
    }

    private void OnBrandChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(BrandItemViewModel.SourceModel) || sender is not BrandItemViewModel brand) return;
        // A new source model keeps the selected camera profiles it also has and drops the rest.
        var keep = _items.Remove(brand, out var old) ? old.Where(i => i.IsSelected).Select(i => i.Name).ToHashSet() : [];
        foreach (var item in old ?? []) { item.PropertyChanged -= OnProfileChanged; Selected.Remove(item); }
        foreach (var item in ItemsOf(brand).Where(i => keep.Contains(i.Name))) item.IsSelected = true;
        RebuildProfiles();
    }

    private void OnProfileChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CameraProfileItemViewModel.IsSelected) || sender is not CameraProfileItemViewModel item) return;
        if (item.IsSelected && !Selected.Contains(item)) Selected.Add(item);
        else if (!item.IsSelected) Selected.Remove(item);
    }

    private void OnSelectionChanged()
    {
        var n = Selected.Count;
        InstallLabel = n > 0 ? $"Install {Text.Profiles(n)}" : "Install";
        TrayChips = Selected.Take(MaxChips).ToList();
        MoreText = n > MaxChips ? $"+{n - MaxChips} more" : null;
        UpdateCounts();
        InstallCommand.NotifyCanExecuteChanged();
    }

    private HashSet<string> InstalledNames() =>
        _store.All.Where(i => i.TargetModel == TargetCamera).Select(i => i.Name).ToHashSet();

    private void RefreshInstalledFlags()
    {
        var installed = InstalledNames();
        foreach (var item in _items.Values.SelectMany(l => l)) item.IsInstalled = installed.Contains(item.Name);
    }

    [RelayCommand]
    private async Task SetTargetAsync(string model, CancellationToken ct)
    {
        TargetCamera = model;
        _settings.TargetCamera = model;
        MarkYourBrand();
        RefreshInstalledFlags();
        try
        {
            await _settings.SaveAsync(ct);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The choice still applies for this session.
            _logger.LogError(ex, "Saving target camera {Model} failed", model);
            Status = new StatusMessage(StatusSeverity.Error, "Couldn't save your camera.", $"{ex.Message} It applies until you close the app.");
            return;
        }
        _logger.LogInformation("Target camera set to {Model}", model);
        Status = new StatusMessage(StatusSeverity.Success, $"Your camera is now {model}.", "Camera profiles you install will show up for this camera.");
    }

    [RelayCommand]
    private void ToggleProfile(CameraProfileItemViewModel profile) => profile.IsSelected = !profile.IsSelected;

    [RelayCommand]
    private void ToggleAllVisible()
    {
        var select = !Profiles.All(p => p.IsSelected);
        foreach (var p in Profiles.ToList()) p.IsSelected = select;
    }

    [RelayCommand]
    private void ClearSearch() => SearchQuery = "";

    [RelayCommand]
    private void ClearSelection()
    {
        var previous = Selected.ToList();
        foreach (var p in previous) p.IsSelected = false;
        Status = new StatusMessage(StatusSeverity.Info, "Selection cleared.", "",
            [new StatusAction("Undo", new RelayCommand(() =>
            {
                foreach (var p in previous) p.IsSelected = true;
                Status = null;
            }))]);
    }

    private bool CanInstall() => !IsBusy && TargetCamera is not null && Selected.Count > 0;

    /// <summary>Installs one by one. A failure or cancel rolls back everything this run added.</summary>
    [RelayCommand(CanExecute = nameof(CanInstall), IncludeCancelCommand = true)]
    private async Task InstallAsync(CancellationToken ct)
    {
        var list = Selected.ToList();
        var target = TargetCamera!;
        var added = new List<InstalledCameraProfile>();
        CameraProfileItemViewModel? current = null;
        IsBusy = true;
        Status = null;
        try
        {
            for (var i = 0; i < list.Count; i++)
            {
                current = list[i];
                ProgressText = $"Installing {i + 1} of {list.Count}: {current.Name}";
                var installed = await _installer.InstallAsync(current.Profile, current.Source.Name, target, ct);
                // The file is on disk now: record it even if cancel arrives, so the rollback finds it.
                await _store.AddAsync(installed, CancellationToken.None);
                added.Add(installed);
            }
            foreach (var p in list) p.IsSelected = false;
            Status = new StatusMessage(StatusSeverity.Success, $"Installed {Text.Profiles(list.Count)} for {target}.",
                "Restart Lightroom or Photoshop to use them.",
            [
                new StatusAction("View installed", new RelayCommand(() => ViewInstalledRequested?.Invoke(this, EventArgs.Empty))),
                new StatusAction("Undo", new AsyncRelayCommand(async () =>
                {
                    Status = await RollbackAsync(added)
                        ? new StatusMessage(StatusSeverity.Info, "Install undone.", $"Removed {Text.Profiles(added.Count)}.")
                        : new StatusMessage(StatusSeverity.Error, "Couldn't undo the install.", RollbackFailed);
                })),
            ]);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Install cancelled after {Done} of {Total} camera profiles", added.Count, list.Count);
            Status = await RollbackAsync(added)
                ? new StatusMessage(StatusSeverity.Info, "Install cancelled.", "No camera profiles were added.")
                : new StatusMessage(StatusSeverity.Error, "Install cancelled.", RollbackFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Installing {Name} from {Source} for {Target} failed", current?.Name, current?.Source.Name, target);
            var rolledBack = await RollbackAsync(added);
            Status = new StatusMessage(StatusSeverity.Error, $"Couldn't install {current?.Name}.", $"{ex.Message} {(rolledBack ? "Nothing was installed." : RollbackFailed)}",
                [new StatusAction("Try again", InstallCommand)]);
        }
        finally
        {
            IsBusy = false;
            ProgressText = null;
        }
    }

    private const string RollbackFailed = "Some installed camera profiles could not be removed. Remove them on the Installed page.";

    /// <summary>Removes what an install added. False when the files could not be removed (logged).</summary>
    private async Task<bool> RollbackAsync(List<InstalledCameraProfile> added)
    {
        try
        {
            // No token: a cancel between the file moves and the manifest save would leave them out of sync.
            await _store.RemoveAsync(added, CancellationToken.None);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Removing {Count} installed camera profiles failed", added.Count);
            return false;
        }
    }
}
