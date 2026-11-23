using Unlocker.Core.Models;
using Unlocker.Presentation.Settings;
using Unlocker.Presentation.Shell;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Presentation;

[TestClass]
public sealed class ShellViewModelTests
{
    [TestMethod]
    public async Task Initialize_loads_store_and_checks_updates()
    {
        var store = new FakeStore();
        await store.AddAsync(new InstalledCameraProfile("ACROS", "Fujifilm X-T5", "Sony ILCE-7M4", [@"C:\out\ACROS"]));
        var updates = new FakeUpdates { Next = new ReleaseInfo(new Version(9, 0), new Uri("https://x/y")) };
        var settings = new SettingsViewModel(updates, new FakeLauncher(), store);
        var shell = new ShellViewModel(store, settings);

        await shell.InitializeAsync();
        await settings.CheckForUpdatesCommand.ExecutionTask!;

        Assert.AreEqual(1, shell.InstalledCount);
        Assert.IsTrue(settings.IsUpdateAvailable);
        Assert.AreEqual("Version 9.0 is available.", settings.UpdateStatus);
    }
}
