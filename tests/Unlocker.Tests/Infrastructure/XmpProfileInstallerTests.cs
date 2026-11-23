using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Installing;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class XmpProfileInstallerTests
{
    [TestMethod]
    public void Rewrite_applies_shared_rules_new_uuid_and_standard_base_profile()
    {
        var tags = new Dictionary<string, string>
        {
            ["Name"] = "Camera BLEACH BYPASS",
            ["UUID"] = "24C5377485D8445092F11C94ABB3938C",
            ["CameraProfile"] = "Camera PROVIA/Standard",
        };

        new XmpProfileInstaller(AppPaths.Create("p", "l"), new CameraProfileRewriter()).Rewrite(tags, "Fujifilm X-T5", "Sony ILCE-7M4");

        Assert.AreEqual("Fujifilm X-T5: BLEACH BYPASS", tags["Name"]);
        Assert.AreEqual("Camera Standard", tags["CameraProfile"]);
        StringAssert.Matches(tags["UUID"], new System.Text.RegularExpressions.Regex("^[0-9A-F]{32}$"));
        Assert.AreNotEqual("24C5377485D8445092F11C94ABB3938C", tags["UUID"]);
    }
}
