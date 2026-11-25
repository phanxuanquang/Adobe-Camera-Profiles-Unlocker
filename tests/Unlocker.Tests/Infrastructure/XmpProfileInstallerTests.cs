using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Installing;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class XmpProfileInstallerTests : TempFolderTest
{
    [TestMethod]
    public void Rewrite_applies_shared_rules_new_uuid_and_adobe_standard_base_without_camera_standard()
    {
        var tags = new Dictionary<string, string>
        {
            ["Name"] = "Camera BLEACH BYPASS",
            ["UUID"] = "24C5377485D8445092F11C94ABB3938C",
            ["CameraProfile"] = "Camera PROVIA/Standard",
        };

        Xmp().Rewrite(tags, "Fujifilm X-T5", "Sony ILCE-7M4");

        Assert.AreEqual("Fujifilm X-T5: BLEACH BYPASS", tags["Name"]);
        Assert.AreEqual("Adobe Standard", tags["CameraProfile"]);
        StringAssert.Matches(tags["UUID"], new System.Text.RegularExpressions.Regex("^[0-9A-F]{32}$"));
        Assert.AreNotEqual("24C5377485D8445092F11C94ABB3938C", tags["UUID"]);
    }

    [TestMethod]
    public void Rewrite_uses_camera_standard_base_when_target_has_one()
    {
        Touch(Paths.CameraProfileFile("Sony ILCE-7M4", "Camera Standard"));

        Assert.AreEqual("Camera Standard", Base("Camera PROVIA/Standard", "Fujifilm X-T5", "Sony ILCE-7M4"));
    }

    [TestMethod]
    public void Rewrite_prefers_camera_standard_v2_base()
    {
        Touch(Paths.CameraProfileFile("Nikon Z 6", "Camera Standard"));
        Touch(Paths.CameraProfileFile("Nikon Z 6", "Camera Standard v2"));

        Assert.AreEqual("Camera Standard v2", Base("Camera PROVIA/Standard", "Fujifilm X-T5", "Nikon Z 6"));
    }

    [TestMethod]
    public void Rewrite_keeps_source_base_when_target_has_it()
    {
        Touch(Paths.CameraProfileFile("Panasonic DC-S5M2", "Camera Standard"));
        Touch(Paths.CameraProfileFile("Panasonic DC-S5M2", "Camera Cinelike D2"));

        Assert.AreEqual("Camera Cinelike D2", Base("Camera Cinelike D2", "Panasonic DC-GH6", "Panasonic DC-S5M2"));
    }

    [TestMethod]
    public void Rewrite_keeps_built_in_base_for_same_brand_only()
    {
        Assert.AreEqual("Camera PROVIA/Standard", Base("Camera PROVIA/Standard", "Fujifilm X-T5", "FUJIFILM X-E5"));
        Assert.AreEqual("Adobe Standard", Base("Camera PROVIA/Standard", "Fujifilm X-T5", "Sony ILCE-7M4"));
    }

    [TestMethod]
    public void Rewrite_copies_source_base_the_target_lacks()
    {
        Touch(Paths.CameraProfileFile("Nikon Z 6", "Camera Standard v2"));
        Touch(Paths.CameraProfileFile("Nikon Z f", "Camera Standard"));

        Assert.AreEqual("Nikon Z 6: Standard v2", Base("Camera Standard v2", "Nikon Z 6", "Nikon Z f"));
    }

    [TestMethod]
    public void Rewrite_copies_source_base_across_brands_even_when_target_has_the_same_name()
    {
        Touch(Paths.CameraProfileFile("Nikon Z f", "Camera Standard"));
        Touch(Paths.CameraProfileFile("Canon EOS R6", "Camera Standard"));

        Assert.AreEqual("Nikon Z f: Standard", Base("Camera Standard", "Nikon Z f", "Canon EOS R6"));
    }

    [TestMethod]
    public void Rewrite_does_not_keep_unknown_missing_base_for_same_brand()
    {
        Touch(Paths.CameraProfileFile("Nikon Z f", "Camera Standard"));

        Assert.AreEqual("Camera Standard", Base("Camera Standard v2", "Nikon D780", "Nikon Z f"));
    }

    [TestMethod]
    public void Rewrite_finds_underscore_file_names()
    {
        Touch(Paths.CameraProfileFile("Leica D-Lux 7", "Camera_Standard_v2"));

        Assert.AreEqual("Camera Standard v2", Base("Camera PROVIA/Standard", "Fujifilm X-T5", "Leica D-Lux 7"));
    }

    [TestMethod]
    public void Rewrite_leaves_missing_base_missing()
    {
        var tags = new Dictionary<string, string> { ["Name"] = "Camera Monochrome Soft" };

        Xmp().Rewrite(tags, "RICOH GR IV Monochrome", "Sony ILCE-7M4");

        Assert.IsFalse(tags.ContainsKey("CameraProfile"));
    }

    private XmpProfileInstaller Xmp() => new(Paths, new CameraProfileRewriter(),
        new DcpProfileInstaller(Paths, new CameraProfileRewriter(), NullLogger<DcpProfileInstaller>.Instance));

    private string Base(string sourceBase, string sourceModel, string targetModel)
    {
        var tags = new Dictionary<string, string> { ["Name"] = "Camera X", ["CameraProfile"] = sourceBase };
        Xmp().Rewrite(tags, sourceModel, targetModel);
        return tags["CameraProfile"];
    }
}
