using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Installing;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class DcpProfileInstallerTests
{
    [TestMethod]
    public void Rewrite_applies_shared_rules_and_replaces_adobe_signature()
    {
        var tags = new Dictionary<string, string> { ["Name"] = "Camera Portrait", ["ProfileCalibrationSignature"] = "com.adobe" };

        new DcpProfileInstaller(AppPaths.Create("p", "l"), new CameraProfileRewriter(), NullLogger<DcpProfileInstaller>.Instance).Rewrite(tags, "Canon EOS R5", "Sony ILCE-7M4");

        Assert.AreEqual("Canon EOS R5: Portrait", tags["Name"]);
        Assert.AreEqual(CameraProfileRewriter.AuthorSignature, tags["ProfileCalibrationSignature"]);
    }
}
