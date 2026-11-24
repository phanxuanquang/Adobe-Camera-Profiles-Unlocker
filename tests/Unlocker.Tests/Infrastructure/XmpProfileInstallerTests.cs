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

        new XmpProfileInstaller(Paths, new CameraProfileRewriter()).Rewrite(tags, "Fujifilm X-T5", "Sony ILCE-7M4");

        Assert.AreEqual("Fujifilm X-T5: BLEACH BYPASS", tags["Name"]);
        Assert.AreEqual("Adobe Standard", tags["CameraProfile"]);
        StringAssert.Matches(tags["UUID"], new System.Text.RegularExpressions.Regex("^[0-9A-F]{32}$"));
        Assert.AreNotEqual("24C5377485D8445092F11C94ABB3938C", tags["UUID"]);
    }

    [TestMethod]
    public void Rewrite_uses_camera_standard_base_when_target_has_one()
    {
        Touch(Paths.CameraStandardFile("Sony ILCE-7M4"));
        var tags = new Dictionary<string, string> { ["Name"] = "Camera ACROS", ["CameraProfile"] = "Camera PROVIA/Standard" };

        new XmpProfileInstaller(Paths, new CameraProfileRewriter()).Rewrite(tags, "Fujifilm X-T5", "Sony ILCE-7M4");

        Assert.AreEqual("Camera Standard", tags["CameraProfile"]);
    }
}
