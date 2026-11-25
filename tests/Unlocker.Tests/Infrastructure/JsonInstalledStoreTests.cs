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
    public async Task Add_keeps_same_name_from_different_source()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);

        await store.AddAsync(Installed("A", "Sony", "a1.xmp"));
        await store.AddAsync(Installed("A", "Sony", "a2.xmp") with { SourceModel = "Fujifilm X-H2" });

        Assert.AreEqual(2, store.All.Count);
    }

    [TestMethod]
    public async Task Load_skips_null_and_incomplete_entries()
    {
        Directory.CreateDirectory(Paths.AppDataDir);
        File.WriteAllText(Paths.ManifestFile, """[null, {"Name":"X"}, {"Name":"Y","SourceModel":"S","TargetModel":"T"}]""");
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);

        await store.LoadAsync();

        Assert.AreEqual(0, store.All.Count);
    }

    [TestMethod]
    public async Task Restore_skips_item_already_reinstalled()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var a = Installed("A", "Sony", "a.xmp");
        await store.AddAsync(a);
        await store.RemoveAsync([a]);
        File.WriteAllText(a.Files[0], "new");
        await store.AddAsync(a with { Files = [a.Files[0]] });

        await store.RestoreAsync([a]);

        Assert.AreEqual(1, store.All.Count);
        Assert.AreEqual("new", File.ReadAllText(a.Files[0]));
        Assert.AreEqual(1, Directory.GetFiles(Paths.TrashDir).Length);
    }

    [TestMethod]
    public async Task Restore_never_overwrites_existing_file()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var a = Installed("A", "Sony", "a.xmp");
        await store.AddAsync(a);
        await store.RemoveAsync([a]);
        File.WriteAllText(a.Files[0], "someone else");

        await store.RestoreAsync([a]);

        Assert.AreEqual(0, store.All.Count);
        Assert.AreEqual("someone else", File.ReadAllText(a.Files[0]));
        Assert.AreEqual(1, Directory.GetFiles(Paths.TrashDir).Length);
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
    public async Task Load_with_locked_manifest_starts_empty()
    {
        Directory.CreateDirectory(Paths.AppDataDir);
        File.WriteAllText(Paths.ManifestFile, "[]");
        using var locked = new FileStream(Paths.ManifestFile, FileMode.Open, FileAccess.Read, FileShare.None);
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);

        await store.LoadAsync();

        Assert.AreEqual(0, store.All.Count);
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
    public async Task Remove_keeps_a_shared_file_while_another_profile_uses_it_and_restore_works()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var a = Installed("A", "Sony", "a.xmp", "base.dcp");
        var b = Installed("B", "Sony", "b.xmp", "base.dcp");
        await store.AddAsync(a);
        await store.AddAsync(b);

        await store.RemoveAsync([a]);

        Assert.IsFalse(File.Exists(a.Files[0]));
        Assert.IsTrue(File.Exists(a.Files[1]), "B still uses the base");

        await store.RestoreAsync([a]); // the base on disk is B's, not a conflict

        Assert.AreEqual(2, store.All.Count);
        Assert.IsTrue(File.Exists(a.Files[0]));

        await store.RemoveAsync([a, b]);

        Assert.IsFalse(File.Exists(a.Files[1]), "last user removed");
        await store.RestoreAsync([a, b]);
        Assert.IsTrue(a.Files.Concat(b.Files).All(File.Exists));

        File.Delete(a.Files[0]); // the user deletes A's XMP by hand; the shared base stays for B
        await store.LoadAsync();
        CollectionAssert.AreEqual(new[] { "B" }, store.All.Select(p => p.Name).ToArray());
    }

    [TestMethod]
    public async Task Load_drops_entries_whose_files_are_gone()
    {
        var first = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var kept = Installed("Kept", "Sony", "kept.xmp");
        var gone = Installed("Gone", "Sony", "gone.xmp");
        await first.AddAsync(kept);
        await first.AddAsync(gone);
        File.Delete(gone.Files[0]);

        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        await store.LoadAsync();

        Assert.AreEqual("Kept", store.All.Single().Name);
    }

    [TestMethod]
    public async Task Dispose_empties_trash()
    {
        var store = new JsonInstalledStore(Paths, NullLogger<JsonInstalledStore>.Instance);
        var a = Installed("A", "Sony", "a.xmp");
        await store.AddAsync(a);
        await store.RemoveAsync([a]);

        store.Dispose();

        Assert.IsFalse(Directory.Exists(Paths.TrashDir));
    }
}
