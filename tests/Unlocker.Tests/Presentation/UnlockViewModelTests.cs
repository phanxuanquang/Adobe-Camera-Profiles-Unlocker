using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Presentation.Common;
using Unlocker.Presentation.Settings;
using Unlocker.Presentation.Unlock;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Presentation;

[TestClass]
public sealed class UnlockViewModelTests
{
    private static CameraProfile P(string name) => new(name, $@"C:\src\{name}", CameraProfileFormat.XMP);

    private static readonly Brand Fujifilm = new("Fujifilm", "Film Simulations",
    [
        new CameraModel("Fujifilm X-T5", [P("CLASSIC CHROME"), P("ACROS"), P("Velvia")]),
        new CameraModel("Fujifilm X-T1", [P("ACROS"), P("Velvia")]),
    ]);

    private static readonly Brand Sony = new("Sony", "Creative Looks",
        [new CameraModel("Sony ILCE-7M4", [P("FL"), P("IN")])]);

    private readonly FakeInstaller _installer = new();
    private readonly FakeStore _store = new();
    private readonly FakeSettings _settings = new() { TargetCamera = "Sony ILCE-7M4" };
    private readonly FakeDialogs _dialogs = new() { Answer = false };
    private readonly FakeLauncher _launcher = new();

    private async Task<UnlockViewModel> Unlock()
    {
        var vm = new UnlockViewModel(new FakeCatalog(Fujifilm, Sony), _installer, _store, _settings, _dialogs, _launcher, NullLogger<UnlockViewModel>.Instance);
        await vm.LoadAsync();
        return vm;
    }

    private static CameraProfileItemViewModel Tile(UnlockViewModel vm, string name) => vm.Profiles.Single(p => p.Name == name);

    [TestMethod]
    public async Task Load_marks_your_brand_and_opens_first_other_brand_with_best_model()
    {
        var vm = await Unlock();

        Assert.IsTrue(vm.Brands.Single(b => b.Name == "Sony").IsYourBrand);
        Assert.AreEqual("Fujifilm", vm.SelectedBrand!.Name);
        Assert.AreEqual("Fujifilm X-T5", vm.SelectedBrand.SourceModel.Name);
        CollectionAssert.AreEqual(new[] { "CLASSIC CHROME", "ACROS", "Velvia" }, vm.Profiles.Select(p => p.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "Fujifilm", "Sony" }, vm.VisibleBrands.Select(b => b.Name).ToArray());
        Assert.AreEqual("Fujifilm Film Simulations", vm.ProfilesTitle);
        Assert.AreEqual("Select all", vm.ToggleAllLabel);
    }

    [TestMethod]
    public async Task Selecting_updates_tray_counts_and_install_state()
    {
        var vm = await Unlock();
        Assert.IsFalse(vm.InstallCommand.CanExecute(null));

        Tile(vm, "ACROS").IsSelected = true;

        CollectionAssert.AreEqual(new[] { "ACROS" }, vm.Selected.Select(p => p.Name).ToArray());
        Assert.AreEqual(1, vm.SelectedBrand!.SelectedCount);
        Assert.AreEqual("Install 1 camera profile", vm.InstallLabel);
        Assert.IsTrue(vm.InstallCommand.CanExecute(null));

        vm.ToggleProfileCommand.Execute(Tile(vm, "ACROS"));

        Assert.AreEqual(0, vm.Selected.Count);
        Assert.AreEqual("Install", vm.InstallLabel);
    }

    [TestMethod]
    public async Task Brand_accessible_name_includes_selected_count()
    {
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;

        Assert.AreEqual("Fujifilm, Film Simulations, 1 selected of 3 camera profiles", vm.SelectedBrand!.ToString());
    }

    [TestMethod]
    public async Task Toggle_all_selects_visible_then_clears_them()
    {
        var vm = await Unlock();

        vm.ToggleAllVisibleCommand.Execute(null);
        Assert.AreEqual(3, vm.Selected.Count);
        Assert.AreEqual("Clear choices", vm.ToggleAllLabel);

        vm.ToggleAllVisibleCommand.Execute(null);
        Assert.AreEqual(0, vm.Selected.Count);
    }

    [TestMethod]
    public async Task Search_spans_brands_and_counts_never_exceed_visible()
    {
        var vm = await Unlock();
        Tile(vm, "CLASSIC CHROME").IsSelected = true;
        Tile(vm, "ACROS").IsSelected = true;

        vm.SearchQuery = "acr";

        Assert.IsTrue(vm.IsSearching);
        Assert.AreEqual("1 camera profile match \"acr\"", vm.ProfilesTitle);
        Assert.AreEqual("Fujifilm Film Simulations", vm.SearchResults.Single().Title);
        var fuji = vm.Brands.Single(b => b.Name == "Fujifilm");
        Assert.AreEqual((1, 1), (fuji.VisibleCount, fuji.SelectedCount));

        vm.SearchQuery = "zzz";
        Assert.IsTrue(vm.IsSearchEmpty);
        Assert.AreEqual("No camera profiles match \"zzz\"", vm.ProfilesTitle);

        vm.ClearSearchCommand.Execute(null);
        Assert.IsFalse(vm.IsSearching);
        Assert.AreEqual(3, vm.Profiles.Count);
        Assert.AreEqual(2, vm.Selected.Count);
    }

    [TestMethod]
    public async Task Search_hides_brands_without_matches_and_clearing_restores_them()
    {
        var vm = await Unlock();

        vm.SearchQuery = "acr";
        CollectionAssert.AreEqual(new[] { "Fujifilm" }, vm.VisibleBrands.Select(b => b.Name).ToArray());

        vm.SearchQuery = "zzz";
        Assert.AreEqual(0, vm.VisibleBrands.Count);

        vm.SearchQuery = "";
        CollectionAssert.AreEqual(new[] { "Fujifilm", "Sony" }, vm.VisibleBrands.Select(b => b.Name).ToArray());
    }

    [TestMethod]
    public void Brand_model_labels_mark_best_model_and_label_selects_source()
    {
        var brand = new BrandItemViewModel(Fujifilm);
        var changed = new List<string?>();
        brand.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        CollectionAssert.AreEqual(new[] { "Fujifilm X-T5 (most camera profiles)", "Fujifilm X-T1" }, brand.ModelLabels.ToArray());
        Assert.AreEqual("Fujifilm X-T5 (most camera profiles)", brand.SourceLabel);
        brand.SourceLabel = "Fujifilm X-T1";
        Assert.AreEqual("Fujifilm X-T1", brand.SourceModel.Name);
        CollectionAssert.Contains(changed, nameof(BrandItemViewModel.SourceLabel));
        // While the brand changes, the ComboBox pushes null or another brand's label: both are ignored.
        brand.SourceLabel = null;
        brand.SourceLabel = "Sony ILCE-7M4";
        Assert.AreEqual("Fujifilm X-T1", brand.SourceModel.Name);
        CollectionAssert.AreEqual(new[] { "Sony ILCE-7M4" }, new BrandItemViewModel(Sony).ModelLabels.ToArray());
    }

    [TestMethod]
    public async Task Changing_source_model_keeps_shared_profiles_and_drops_missing()
    {
        var vm = await Unlock();
        Tile(vm, "CLASSIC CHROME").IsSelected = true;
        Tile(vm, "ACROS").IsSelected = true;

        vm.SelectedBrand!.SourceModel = Fujifilm.Models[1];

        CollectionAssert.AreEqual(new[] { "ACROS" }, vm.Selected.Select(p => p.Name).ToArray());
        Assert.AreEqual("Fujifilm X-T1", vm.Selected[0].Source.Name);
        Assert.IsTrue(Tile(vm, "ACROS").IsSelected);
        Assert.AreEqual(2, vm.Profiles.Count);
    }

    [TestMethod]
    public async Task Clear_selection_offers_undo()
    {
        var vm = await Unlock();
        vm.ToggleAllVisibleCommand.Execute(null);

        vm.ClearSelectionCommand.Execute(null);

        Assert.AreEqual(0, vm.Selected.Count);
        Assert.AreEqual("Selection cleared.", vm.Status!.Title);
        vm.Status.Actions!.Single(a => a.Label == "Undo").Command.Execute(null);
        Assert.AreEqual(3, vm.Selected.Count);
        Assert.IsNull(vm.Status);
    }

    [TestMethod]
    public async Task Tray_shows_four_chips_then_more()
    {
        var vm = await Unlock();
        vm.ToggleAllVisibleCommand.Execute(null);
        vm.SelectedBrand = vm.Brands.Single(b => b.Name == "Sony");
        vm.ToggleAllVisibleCommand.Execute(null);

        Assert.AreEqual(4, vm.TrayChips.Count);
        Assert.AreEqual("+1 more", vm.MoreText);
    }

    [TestMethod]
    public async Task Buy_me_a_coffee_in_success_dialog_opens_donate_page()
    {
        _dialogs.Answer = true;
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.AreEqual("Buy me a coffee", _dialogs.LastConfirm);
        CollectionAssert.AreEqual(new[] { SettingsViewModel.DonateUrl.AbsoluteUri }, _launcher.Opened);
    }

    [TestMethod]
    public async Task Install_adds_to_store_clears_selection_and_undo_removes()
    {
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;
        Tile(vm, "Velvia").IsSelected = true;

        await vm.InstallCommand.ExecuteAsync(null);

        CollectionAssert.AreEqual(new[] { "ACROS", "Velvia" }, _store.All.Select(p => p.Name).ToArray());
        Assert.IsTrue(_store.All.All(p => p.TargetModel == "Sony ILCE-7M4" && p.SourceModel == "Fujifilm X-T5"));
        Assert.AreEqual(0, vm.Selected.Count);
        Assert.IsTrue(Tile(vm, "ACROS").IsInstalled);
        Assert.IsFalse(vm.IsBusy);
        Assert.AreEqual(StatusSeverity.Success, vm.Status!.Severity);
        Assert.AreEqual("Install finished.", vm.Status.Title);
        Assert.AreEqual("Camera profiles installed", _dialogs.LastTitle);
        StringAssert.StartsWith(_dialogs.LastBody, "Installed 2 camera profiles for Sony ILCE-7M4.");
        Assert.AreEqual(0, _launcher.Opened.Count);

        await ((IAsyncRelayCommand)vm.Status.Actions!.Single(a => a.Label == "Undo").Command).ExecuteAsync(null);

        Assert.AreEqual(0, _store.All.Count);
        Assert.IsFalse(Tile(vm, "ACROS").IsInstalled);
        Assert.AreEqual("Install undone.", vm.Status!.Title);
    }

    [TestMethod]
    public async Task Installed_flag_needs_same_source_model()
    {
        await _store.AddAsync(new InstalledCameraProfile("ACROS", "Fujifilm X-T1", "Sony ILCE-7M4", []));
        await _store.AddAsync(new InstalledCameraProfile("CLASSIC CHROME", "Fujifilm X-T5", "Nikon Z 8", []));

        var vm = await Unlock();

        Assert.IsFalse(Tile(vm, "ACROS").IsInstalled);
        Assert.IsFalse(Tile(vm, "CLASSIC CHROME").IsInstalled);
    }

    [TestMethod]
    public async Task Profile_that_becomes_installed_leaves_the_selection()
    {
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;

        await _store.AddAsync(new InstalledCameraProfile("ACROS", "Fujifilm X-T5", "Sony ILCE-7M4", []));

        Assert.IsTrue(Tile(vm, "ACROS").IsInstalled);
        Assert.IsFalse(Tile(vm, "ACROS").IsSelected);
        Assert.AreEqual(0, vm.Selected.Count);
    }

    [TestMethod]
    public async Task Success_dialog_failure_keeps_install_and_status()
    {
        _dialogs.FailWith = new InvalidOperationException("Only a single ContentDialog can be open at any time.");
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.AreEqual(1, _store.All.Count);
        Assert.AreEqual("Install finished.", vm.Status!.Title);
    }

    [TestMethod]
    public async Task Select_all_skips_installed_profiles()
    {
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;
        await vm.InstallCommand.ExecuteAsync(null);

        vm.ToggleAllVisibleCommand.Execute(null);

        CollectionAssert.AreEquivalent(new[] { "CLASSIC CHROME", "Velvia" }, vm.Selected.Select(p => p.Name).ToArray());
        Assert.AreEqual("Clear choices", vm.ToggleAllLabel);
    }

    [TestMethod]
    public async Task Undo_after_removal_elsewhere_removes_nothing_and_clears_status()
    {
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;
        await vm.InstallCommand.ExecuteAsync(null);
        var undo = (IAsyncRelayCommand)vm.Status!.Actions!.Single(a => a.Label == "Undo").Command;
        await _store.RemoveAsync(_store.All.ToList()); // the Installed page removed it

        await undo.ExecuteAsync(null);

        Assert.IsNull(vm.Status);
    }

    [TestMethod]
    public async Task Install_failure_rolls_back_keeps_selection_and_shows_error()
    {
        var vm = await Unlock();
        vm.ToggleAllVisibleCommand.Execute(null);
        _installer.FailWhen = p => p.Name == "Velvia" ? new InvalidOperationException("DCP file is truncated or corrupt.") : null;

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.AreEqual(0, _store.All.Count);
        Assert.AreEqual(3, vm.Selected.Count);
        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
        Assert.AreEqual("Couldn't install Velvia.", vm.Status.Title);
        StringAssert.Contains(vm.Status.Message, "DCP file is truncated or corrupt.");
        Assert.AreEqual("Try again", vm.Status.Actions!.Single().Label);
    }

    [TestMethod]
    public async Task Undo_failure_shows_error()
    {
        var vm = await Unlock();
        Tile(vm, "ACROS").IsSelected = true;
        await vm.InstallCommand.ExecuteAsync(null);
        _store.FailRemoveWith = new UnauthorizedAccessException("Access denied.");

        await ((IAsyncRelayCommand)vm.Status!.Actions!.Single(a => a.Label == "Undo").Command).ExecuteAsync(null);

        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
        Assert.AreEqual("Couldn't undo the install.", vm.Status.Title);
    }

    [TestMethod]
    public async Task Rollback_failure_still_shows_install_error()
    {
        var vm = await Unlock();
        vm.ToggleAllVisibleCommand.Execute(null);
        _installer.FailWhen = p => p.Name == "Velvia" ? new IOException("Disk full.") : null;
        _store.FailRemoveWith = new IOException("File in use.");

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
        Assert.AreEqual("Couldn't install Velvia.", vm.Status.Title);
        StringAssert.Contains(vm.Status.Message, "Some installed camera profiles could not be removed");
    }

    [TestMethod]
    public async Task Set_target_save_failure_shows_error()
    {
        var vm = await Unlock();
        _settings.FailSaveWith = new IOException("Disk full.");

        await vm.SetTargetCommand.ExecuteAsync("Fujifilm X-T5");

        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
        Assert.AreEqual("Couldn't save your camera.", vm.Status.Title);
    }

    [TestMethod]
    public async Task Install_cancel_rolls_back()
    {
        var vm = await Unlock();
        vm.ToggleAllVisibleCommand.Execute(null);
        _installer.FailWhen = p => p.Name == "Velvia" ? new OperationCanceledException() : null;

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.AreEqual(0, _store.All.Count);
        Assert.AreEqual("Install cancelled.", vm.Status!.Title);
    }

    [TestMethod]
    public async Task Set_target_persists_and_reports()
    {
        _settings.TargetCamera = null;
        var vm = await Unlock();
        Assert.IsTrue(vm.IsFirstRun);

        await vm.SetTargetCommand.ExecuteAsync("Fujifilm X-T5");

        Assert.AreEqual("Fujifilm X-T5", _settings.TargetCamera);
        Assert.IsFalse(vm.IsFirstRun);
        Assert.AreEqual("Your camera is now Fujifilm X-T5.", vm.Status!.Title);
        Assert.IsTrue(vm.Brands.Single(b => b.Name == "Fujifilm").IsYourBrand);
    }
}
