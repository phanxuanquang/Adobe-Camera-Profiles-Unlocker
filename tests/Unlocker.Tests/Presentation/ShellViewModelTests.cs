using Unlocker.Core.Models;
using Unlocker.Presentation.Shell;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests.Presentation;

[TestClass]
public sealed class ShellViewModelTests
{
    [TestMethod]
    public async Task Initialize_loads_store()
    {
        var store = new FakeStore();
        await store.AddAsync(new InstalledCameraProfile("ACROS", "Fujifilm X-T5", "Sony ILCE-7M4", [@"C:\out\ACROS"]));
        var shell = new ShellViewModel(store);

        await shell.InitializeAsync();

        Assert.AreEqual(1, store.LoadCount);
        Assert.AreEqual(1, shell.InstalledCount);
    }
}
