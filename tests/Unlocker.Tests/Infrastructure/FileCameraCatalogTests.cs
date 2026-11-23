using Microsoft.Extensions.Logging.Abstractions;
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
}
