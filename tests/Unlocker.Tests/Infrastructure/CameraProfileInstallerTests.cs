using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Installing;
using Unlocker.Tests.Fakes;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class CameraProfileInstallerTests : TempFolderTest
{
    [TestMethod]
    public async Task Dcp_writes_rewritten_copy_to_output_dir()
    {
        var src = Touch(Path.Combine(Paths.DcpProfilesDir, "Canon EOS R5", "Canon EOS R5 Camera Portrait.dcp"));
        var profile = new CameraProfile("Portrait", src, CameraProfileFormat.Dcp);
        var model = new CameraModel("Canon EOS R5", "Canon", [profile]);

        var result = await new CameraProfileInstaller(Paths, new FakeDcpTool(), NullLogger<CameraProfileInstaller>.Instance).InstallAsync(profile, model, "Sony ILCE-7M4");

        var output = Path.Combine(Paths.DcpOutputDir, "Portrait (for Sony ILCE-7M4).dcp");
        CollectionAssert.AreEqual(new[] { output }, result.Files.ToArray());
        Assert.AreEqual("Sony ILCE-7M4", (string?)XDocument.Load(output).Root!.Element("UniqueCameraModelRestriction"));
        Assert.AreEqual("x", File.ReadAllText(src));
        Assert.AreEqual(("Portrait", "Canon EOS R5", "Sony ILCE-7M4", false), (result.Name, result.SourceModel, result.TargetModel, result.IsLegacy));
    }

    [TestMethod]
    public async Task Xmp_writes_rewritten_copy_and_leaves_source_unchanged()
    {
        var src = WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");
        var before = File.ReadAllBytes(src);
        var profile = new CameraProfile("CLASSIC Neg", src, CameraProfileFormat.Xmp);
        var model = new CameraModel("Fujifilm X-T5", "Fujifilm", [profile]);

        var result = await new CameraProfileInstaller(Paths, new FakeDcpTool(), NullLogger<CameraProfileInstaller>.Instance).InstallAsync(profile, model, "Sony ILCE-7M4");

        var output = Path.Combine(Paths.XmpOutputDir, "CLASSIC Neg (for Sony ILCE-7M4).xmp");
        CollectionAssert.AreEqual(new[] { output }, result.Files.ToArray());
        var desc = XDocument.Load(output).Descendants(CameraProfileRewriter.Rdf + "Description").Single();
        Assert.AreEqual("Sony ILCE-7M4", (string?)desc.Attribute(CameraProfileRewriter.Crs + "CameraModelRestriction"));
        CollectionAssert.AreEqual(before, File.ReadAllBytes(src));
    }
}
