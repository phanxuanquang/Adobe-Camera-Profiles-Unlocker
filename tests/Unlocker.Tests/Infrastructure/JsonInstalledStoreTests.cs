using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Infrastructure.Storage;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class JsonInstalledStoreTests : TempFolderTest
{
    private InstalledCameraProfile Installed(string name, string target, params string[] files)
        => new(name, "Fujifilm X-T5", target, files.Select(f => Touch(Path.Combine(Paths.XmpOutputDir, f))).ToList());

    [TestMethod]
    public async Task Add_replaces_same_name_and_target_and_persists()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var changed = 0;
        store.Changed += (_, _) => changed++;

        await store.AddAsync(Installed("A", "Sony", "a1.xmp"));
        await store.AddAsync(Installed("A", "Sony", "a2.xmp"));
        await store.AddAsync(Installed("B", "Sony", "b.xmp"));

        Assert.AreEqual(3, changed);
        CollectionAssert.AreEqual(new[] { "A", "B" }, store.All.Select(p => p.Name).Order().ToArray());
        var reloaded = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        await reloaded.LoadAsync();
        Assert.AreEqual(2, reloaded.All.Count);
    }

    [TestMethod]
    public async Task Cancelled_save_keeps_previous_manifest()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        await store.AddAsync(Installed("A", "Sony", "a.xmp"));

        await Assert.ThrowsAsync<OperationCanceledException>(() => store.AddAsync(Installed("B", "Sony", "b.xmp"), new CancellationToken(canceled: true)));

        var reloaded = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        await reloaded.LoadAsync();
        CollectionAssert.AreEqual(new[] { "A" }, reloaded.All.Select(p => p.Name).ToArray());
        CollectionAssert.AreEqual(new[] { Paths.ManifestFile }, Directory.GetFiles(Paths.AppDataDir));
    }

    [TestMethod]
    public async Task Remove_moves_files_to_trash_and_restore_brings_them_back()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var a = Installed("A", "Sony", "a.xmp");
        await store.AddAsync(a);

        await store.RemoveAsync([a]);

        Assert.AreEqual(0, store.All.Count);
        Assert.IsFalse(File.Exists(a.Files[0]));
        Assert.AreEqual(1, Directory.GetFiles(Paths.TrashDir).Length);

        await store.RestoreAsync([a]);

        Assert.AreEqual(1, store.All.Count);
        Assert.IsTrue(File.Exists(a.Files[0]));
    }

    [TestMethod]
    public async Task Load_drops_missing_files_and_detects_legacy_dcp()
    {
        var first = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var kept = Installed("Kept", "Sony", "kept.xmp");
        var gone = Installed("Gone", "Sony", "gone.xmp");
        await first.AddAsync(kept);
        await first.AddAsync(gone);
        File.Delete(gone.Files[0]);
        Touch(Path.Combine(Paths.LegacyDcpDir, "CLASSIC CHROME (for Sony ILCE-7M4).dcp"));
        Touch(Path.Combine(Paths.LegacyDcpDir, "Someone else.dcp"));

        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        await store.LoadAsync();

        Assert.AreEqual(2, store.All.Count);
        Assert.IsTrue(store.All.Any(p => p.Name == "Kept" && !p.IsLegacy));
        var legacy = store.All.Single(p => p.IsLegacy);
        Assert.AreEqual(("CLASSIC CHROME", "Sony ILCE-7M4"), (legacy.Name, legacy.TargetModel));
    }

    [TestMethod]
    public async Task Dispose_empties_trash()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var a = Installed("A", "Sony", "a.xmp");
        await store.AddAsync(a);
        await store.RemoveAsync([a]);

        await store.DisposeAsync();

        Assert.IsFalse(Directory.Exists(Paths.TrashDir));
    }
}
