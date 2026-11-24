using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.AccessControl;
using System.Security.Principal;
using Unlocker.Core.Enums;
using Unlocker.Infrastructure.Catalog;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class FileCameraCatalogTests : TempFolderTest
{
    [TestMethod]
    public async Task Reads_xmp_model_from_restriction_in_nested_and_flat_folders()
    {
        WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");
        WriteXmp(@"Fujifilm\Fujifilm GFX 100 Camera Sepia.xmp", "Fujifilm GFX 100");

        var brands = await new FileCameraCatalog(Paths, NullLogger<FileCameraCatalog>.Instance).LoadAsync();

        var fuji = brands.Single(b => b.Name == "Fujifilm");
        var xt5 = fuji.Models.Single(m => m.Name == "Fujifilm X-T5").Profiles.Single();
        Assert.AreEqual("CLASSIC Neg", xt5.Name);
        Assert.AreEqual(CameraProfileFormat.XMP, xt5.Format);
        Assert.AreEqual("Sepia", fuji.Models.Single(m => m.Name == "Fujifilm GFX 100").Profiles.Single().Name);
    }

    [TestMethod]
    public async Task Merges_dcp_and_xmp_and_prefers_xmp_on_duplicate_name()
    {
        Touch(Path.Combine(Paths.DcpProfilesDir, "Fujifilm X-T5", "Fujifilm X-T5 Camera CLASSIC Neg.dcp"));
        Touch(Path.Combine(Paths.DcpProfilesDir, "Fujifilm X-T5", "Fujifilm X-T5 Camera PROVIA.dcp"));
        WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");

        var brands = await new FileCameraCatalog(Paths, NullLogger<FileCameraCatalog>.Instance).LoadAsync();

        var profiles = brands.Single().Models.Single().Profiles;
        Assert.AreEqual(2, profiles.Count);
        Assert.AreEqual(CameraProfileFormat.XMP, profiles.Single(p => p.Name == "CLASSIC Neg").Format);
        Assert.AreEqual(CameraProfileFormat.DCP, profiles.Single(p => p.Name == "PROVIA").Format);
    }

    [TestMethod]
    public async Task Skips_unreadable_xmp()
    {
        Touch(Path.Combine(Paths.XmpProfilesDir, "Fujifilm", "broken.xmp"));

        var brands = await new FileCameraCatalog(Paths, NullLogger<FileCameraCatalog>.Instance).LoadAsync();

        Assert.AreEqual(0, brands.Count);
    }

    [TestMethod]
    [DataRow("Leica D-Lux 7 Camera Monochrome.dcp")]
    [DataRow("Leica D-Lux 7 Camera_Monochrome.dcp")]
    public async Task Reads_dcp_name_after_space_or_underscore(string file)
    {
        Touch(Path.Combine(Paths.DcpProfilesDir, "Leica D-Lux 7", file));

        var brands = await new FileCameraCatalog(Paths, NullLogger<FileCameraCatalog>.Instance).LoadAsync();

        Assert.AreEqual("Monochrome", brands.Single().Models.Single().Profiles.Single().Name);
    }

    [TestMethod]
    public async Task Skips_inaccessible_subfolder()
    {
        Touch(Path.Combine(Paths.DcpProfilesDir, "Fujifilm X-T5", "Fujifilm X-T5 Camera PROVIA.dcp"));
        WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");
        var locked = new[] { Path.Combine(Paths.DcpProfilesDir, "Locked"), Path.Combine(Paths.XmpProfilesDir, "Locked") };
        var deny = new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, FileSystemRights.ListDirectory, AccessControlType.Deny);
        foreach (var dir in locked)
        {
            var info = Directory.CreateDirectory(dir);
            var acl = info.GetAccessControl();
            acl.AddAccessRule(deny);
            info.SetAccessControl(acl);
        }
        try
        {
            var brands = await new FileCameraCatalog(Paths, NullLogger<FileCameraCatalog>.Instance).LoadAsync();

            Assert.AreEqual(2, brands.Single().Models.Single().Profiles.Count);
        }
        finally
        {
            foreach (var dir in locked)
            {
                var info = new DirectoryInfo(dir);
                var acl = info.GetAccessControl();
                acl.RemoveAccessRule(deny);
                info.SetAccessControl(acl);
            }
        }
    }

    [TestMethod]
    public async Task Failed_scan_is_retried_on_next_load()
    {
        var catalog = new FileCameraCatalog(Paths, new FailOnceLogger());

        await Assert.ThrowsAsync<IOException>(() => catalog.LoadAsync());

        Assert.AreEqual(0, (await catalog.LoadAsync()).Count);
    }

    /// <summary>Throws on the first log call: stands in for any scan failure.</summary>
    private sealed class FailOnceLogger : ILogger<FileCameraCatalog>
    {
        private bool _failed;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (_failed) return;
            _failed = true;
            throw new IOException("disk gone");
        }
    }
}
