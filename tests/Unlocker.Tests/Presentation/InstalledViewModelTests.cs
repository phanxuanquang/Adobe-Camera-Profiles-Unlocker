using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Common;
using Unlocker.Presentation.Installed;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Presentation;

[TestClass]
public sealed class InstalledViewModelTests
{
    private readonly FakeStore _store = new();

    private static InstalledCameraProfile Installed(string name, string target)
        => new(name, "Fujifilm X-T5", target, [$@"C:\out\{name}"]);

    [TestMethod]
    public async Task Single_remove_is_instant_with_undo()
    {
        await _store.AddAsync(Installed("ACROS", "Sony ILCE-7M4"));
        var vm = new InstalledViewModel(_store, new FakeDialogs { Answer = false }, new FakeLauncher(), NullLogger<InstalledViewModel>.Instance);

        await vm.RemoveCommand.ExecuteAsync(_store.All[0]);

        Assert.IsTrue(vm.IsEmpty);
        Assert.AreEqual("Removed ACROS for Sony ILCE-7M4.", vm.Status!.Title);
        await ((IAsyncRelayCommand)vm.Status.Actions!.Single().Command).ExecuteAsync(null);
        Assert.AreEqual(1, vm.Groups.Single().Items.Count);
        Assert.IsNull(vm.Status);
    }

    [TestMethod]
    public async Task Remove_access_denied_shows_error()
    {
        await _store.AddAsync(Installed("ACROS", "Sony ILCE-7M4"));
        _store.FailRemoveWith = new UnauthorizedAccessException("Access denied.");
        var vm = new InstalledViewModel(_store, new FakeDialogs(), new FakeLauncher(), NullLogger<InstalledViewModel>.Instance);

        await vm.RemoveCommand.ExecuteAsync(_store.All[0]);

        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
    }

    [TestMethod]
    public async Task Undo_failure_shows_error()
    {
        await _store.AddAsync(Installed("ACROS", "Sony ILCE-7M4"));
        var vm = new InstalledViewModel(_store, new FakeDialogs(), new FakeLauncher(), NullLogger<InstalledViewModel>.Instance);
        await vm.RemoveCommand.ExecuteAsync(_store.All[0]);
        _store.FailRestoreWith = new IOException("File in use.");

        await ((IAsyncRelayCommand)vm.Status!.Actions!.Single().Command).ExecuteAsync(null);

        Assert.AreEqual(StatusSeverity.Error, vm.Status!.Severity);
        Assert.AreEqual("Couldn't undo the removal.", vm.Status.Title);
    }

    [TestMethod]
    public async Task Group_remove_asks_first()
    {
        await _store.AddAsync(Installed("ACROS", "Sony ILCE-7M4"));
        await _store.AddAsync(Installed("Velvia", "Sony ILCE-7M4"));
        await _store.AddAsync(Installed("ACROS", "Canon EOS R5"));
        var dialogs = new FakeDialogs { Answer = false };
        var vm = new InstalledViewModel(_store, dialogs, new FakeLauncher(), NullLogger<InstalledViewModel>.Instance);
        var sony = vm.Groups.Single(g => g.TargetModel == "Sony ILCE-7M4");

        await vm.RemoveGroupCommand.ExecuteAsync(sony);
        Assert.AreEqual(3, _store.All.Count);
        Assert.AreEqual("Remove camera profiles for Sony ILCE-7M4?", dialogs.LastTitle);
        Assert.AreEqual("Remove 2", dialogs.LastConfirm);

        dialogs.Answer = true;
        await vm.RemoveGroupCommand.ExecuteAsync(sony);
        Assert.AreEqual(1, _store.All.Count);
        Assert.AreEqual("Removed 2 camera profiles.", vm.Status!.Title);

        await vm.RemoveAllCommand.ExecuteAsync(null);
        Assert.IsTrue(vm.IsEmpty);
        Assert.IsFalse(vm.RemoveAllCommand.CanExecute(null));
    }

    [TestMethod]
    public async Task Open_folder_uses_store_folder()
    {
        var launcher = new FakeLauncher();

        await new InstalledViewModel(_store, new FakeDialogs(), launcher, NullLogger<InstalledViewModel>.Instance).OpenFolderCommand.ExecuteAsync(null);

        CollectionAssert.AreEqual(new[] { _store.Folders[0] }, launcher.Opened);
    }
}
