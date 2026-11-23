using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Common;
using Unlocker.Presentation.Unlock;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Presentation;

[TestClass]
public sealed class UnlockViewModelTests
{
    private static CameraProfile P(string name) => new(name, $@"C:\src\{name}", CameraProfileFormat.Xmp);

    private static readonly Brand Fujifilm = new("Fujifilm", "Film Simulations",
    [
        new CameraModel("Fujifilm X-T5", "Fujifilm", [P("CLASSIC CHROME"), P("ACROS"), P("Velvia")]),
        new CameraModel("Fujifilm X-T1", "Fujifilm", [P("ACROS"), P("Velvia")]),
    ]);

    private static readonly Brand Sony = new("Sony", "Creative Looks",
        [new CameraModel("Sony ILCE-7M4", "Sony", [P("FL"), P("IN")])]);

    private readonly FakeInstaller _installer = new();
    private readonly FakeStore _store = new();
    private readonly FakeSettings _settings = new() { TargetCamera = "Sony ILCE-7M4" };

    private async Task<UnlockViewModel> Unlock()
    {
        var vm = new UnlockViewModel(new FakeCatalog(Fujifilm, Sony), _installer, _store, _settings, NullLogger<UnlockViewModel>.Instance);
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
        Assert.AreEqual("Installed 2 camera profiles for Sony ILCE-7M4.", vm.Status.Title);

        await ((IAsyncRelayCommand)vm.Status.Actions!.Single(a => a.Label == "Undo").Command).ExecuteAsync(null);

        Assert.AreEqual(0, _store.All.Count);
        Assert.IsFalse(Tile(vm, "ACROS").IsInstalled);
        Assert.AreEqual("Install undone.", vm.Status!.Title);
    }

    [TestMethod]
    public async Task Install_failure_rolls_back_keeps_selection_and_shows_error()
    {
        var vm = await Unlock();
        vm.ToggleAllVisibleCommand.Execute(null);
        _installer.FailWhen = p => p.Name == "Velvia" ? new InvalidOperationException("dcpTool stopped responding.") : null;

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.AreEqual(0, _store.All.Count);
        Assert.AreEqual(3, vm.Selected.Count);
        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
        Assert.AreEqual("Couldn't install Velvia.", vm.Status.Title);
        StringAssert.Contains(vm.Status.Message, "dcpTool stopped responding.");
        Assert.AreEqual("Try again", vm.Status.Actions!.Single().Label);
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
