using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Presentation.Settings;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Presentation;

[TestClass]
public sealed class SettingsViewModelTests
{
    private readonly FakeUpdates _updates = new();
    private readonly FakeLauncher _launcher = new();

    private SettingsViewModel Settings() => new(_updates, _launcher, new FakeStore(), NullLogger<SettingsViewModel>.Instance);

    [TestMethod]
    public async Task Newer_release_offers_download_page()
    {
        _updates.Next = new ReleaseInfo(new Version(9, 0), new Uri("https://github.com/x/y/releases/tag/v9.0"));
        var vm = Settings();

        await vm.CheckForUpdatesCommand.ExecuteAsync(null);
        await vm.OpenDownloadPageCommand.ExecuteAsync(null);

        Assert.IsTrue(vm.IsUpdateAvailable);
        Assert.AreEqual("Version 9.0 is available.", vm.UpdateStatus);
        CollectionAssert.AreEqual(new[] { "https://github.com/x/y/releases/tag/v9.0" }, _launcher.Opened);
    }

    [TestMethod]
    public async Task No_newer_release_says_up_to_date()
    {
        var vm = Settings();

        await vm.CheckForUpdatesCommand.ExecuteAsync(null);

        Assert.IsFalse(vm.IsUpdateAvailable);
        Assert.AreEqual("You're up to date.", vm.UpdateStatus);
    }

    [TestMethod]
    public async Task Failed_check_never_says_up_to_date()
    {
        _updates.FailWith = new HttpRequestException("offline");
        var vm = Settings();

        await vm.CheckForUpdatesCommand.ExecuteAsync(null);

        Assert.IsFalse(vm.IsUpdateAvailable);
        StringAssert.StartsWith(vm.UpdateStatus, "Couldn't check for updates.");
    }
}
