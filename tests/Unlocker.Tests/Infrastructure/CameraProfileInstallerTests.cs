using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Installing;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class CameraProfileInstallerTests : TempFolderTest
{
    private CameraProfileInstaller Installer() => new(
        new DcpProfileInstaller(Paths, new CameraProfileRewriter()),
        new XmpProfileInstaller(Paths, new CameraProfileRewriter()),
        NullLogger<CameraProfileInstaller>.Instance);

    [TestMethod]
    public async Task Dcp_writes_rewritten_copy_to_output_dir()
    {
        var src = Path.Combine(Paths.DcpProfilesDir, "Canon EOS R5", "Canon EOS R5 Camera Portrait.dcp");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        var before = DcpFileTests.Sample();
        File.WriteAllBytes(src, before);
        var profile = new CameraProfile("Portrait", src, CameraProfileFormat.DCP);
        var model = new CameraModel("Canon EOS R5", "Canon", [profile]);

        var result = await Installer().InstallAsync(profile, model, "Sony ILCE-7M4");

        var output = Path.Combine(Paths.DcpOutputDir, "Portrait (for Sony ILCE-7M4).dcp");
        CollectionAssert.AreEqual(new[] { output }, result.Files.ToArray());
        CollectionAssert.AreEqual(DcpFileTests.Rewrite(before, t => new DcpProfileInstaller(Paths, new CameraProfileRewriter()).Rewrite(t, "Canon EOS R5", "Sony ILCE-7M4")), File.ReadAllBytes(output));
        var strings = DcpFileTests.ReadStrings(File.ReadAllBytes(output));
        Assert.AreEqual("Sony ILCE-7M4", strings["CameraModelRestriction"]);
        Assert.AreEqual("Canon EOS R5: Portrait", strings["Name"]);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(src));
        Assert.AreEqual(("Portrait", "Canon EOS R5", "Sony ILCE-7M4", false), (result.Name, result.SourceModel, result.TargetModel, result.IsLegacy));
    }

    [TestMethod]
    public async Task Xmp_writes_rewritten_copy_and_leaves_source_unchanged()
    {
        var src = WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");
        var before = File.ReadAllBytes(src);
        var profile = new CameraProfile("CLASSIC Neg", src, CameraProfileFormat.XMP);
        var model = new CameraModel("Fujifilm X-T5", "Fujifilm", [profile]);

        var result = await Installer().InstallAsync(profile, model, "Sony ILCE-7M4");

        var output = Path.Combine(Paths.XmpOutputDir, "CLASSIC Neg (for Sony ILCE-7M4).xmp");
        CollectionAssert.AreEqual(new[] { output }, result.Files.ToArray());
        var (crs, rdf) = (XmpProfileInstaller.Crs, XmpProfileInstaller.Rdf);
        var desc = XDocument.Load(output).Descendants(rdf + "Description").Single();
        var oldDesc = XDocument.Load(src).Descendants(rdf + "Description").Single();
        Assert.AreEqual("Sony ILCE-7M4", (string?)desc.Attribute(crs + "CameraModelRestriction"));
        Assert.AreEqual("Fujifilm X-T5: BLEACH BYPASS", (string?)desc.Element(crs + "Name")!.Descendants(rdf + "li").Single());
        Assert.AreNotEqual((string?)oldDesc.Attribute(crs + "UUID"), (string?)desc.Attribute(crs + "UUID"));
        string[] touched = ["CameraModelRestriction", "Copyright", "UUID", "CameraProfile"];
        CollectionAssert.AreEqual(
            oldDesc.Attributes().Where(a => !touched.Contains(a.Name.LocalName)).Select(a => a.ToString()).ToList(),
            desc.Attributes().Where(a => !touched.Contains(a.Name.LocalName)).Select(a => a.ToString()).ToList());
        CollectionAssert.AreEqual(
            oldDesc.Elements().Skip(1).Select(e => e.ToString()).ToList(),
            desc.Elements().Skip(1).Select(e => e.ToString()).ToList());
        CollectionAssert.AreEqual(before, File.ReadAllBytes(src));
    }
}
