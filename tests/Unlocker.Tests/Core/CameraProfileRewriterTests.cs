using System.Security.Cryptography;
using System.Xml.Linq;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Core;

[TestClass]
public sealed class CameraProfileRewriterTests
{
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    [TestMethod]
    [DataRow("Canon EOS R5 Camera Portrait.dcp", "Canon EOS R5", "Portrait")]
    [DataRow("Fujifilm GFX 100 Camera BLEACH BYPASS.xmp", "Fujifilm GFX 100", "BLEACH BYPASS")]
    [DataRow("Other Camera Portrait.dcp", "Canon EOS R5", null)]
    public void ParseProfileName(string file, string model, string? expected)
        => Assert.AreEqual(expected, CameraProfileRewriter.ParseProfileName(file, model));

    [TestMethod]
    public void OutputFileName_replaces_invalid_chars()
        => Assert.AreEqual("PROVIA-Standard (for Sony ILCE-7M4).xmp",
            CameraProfileRewriter.OutputFileName("PROVIA/Standard", "Sony ILCE-7M4", CameraProfileFormat.Xmp));

    [TestMethod]
    public void RewriteDcpXml_rewrites_name_copyright_signature_restriction()
    {
        var path = TestFiles.Fixture("sample.dcp.xml");
        var before = Hash(path);
        var original = XDocument.Load(path);
        var doc = new XDocument(original);

        CameraProfileRewriter.RewriteDcpXml(doc, "Canon EOS R5", "Sony ILCE-7M4");

        var root = doc.Root!;
        Assert.AreEqual("Canon EOS R5: Portrait", (string?)root.Element("ProfileName"));
        Assert.AreEqual(CameraProfileRewriter.AuthorCredit, (string?)root.Element("Copyright"));
        Assert.AreEqual(CameraProfileRewriter.AuthorSignature, (string?)root.Element("ProfileCalibrationSignature"));
        Assert.AreEqual("Sony ILCE-7M4", (string?)root.Element("UniqueCameraModelRestriction"));
        string[] touched = ["ProfileName", "Copyright", "ProfileCalibrationSignature", "UniqueCameraModelRestriction"];
        CollectionAssert.AreEqual(
            original.Root!.Elements().Where(e => !touched.Contains(e.Name.LocalName)).Select(e => e.ToString()).ToList(),
            root.Elements().Where(e => !touched.Contains(e.Name.LocalName)).Select(e => e.ToString()).ToList());
        Assert.AreEqual(before, Hash(path));
    }

    [TestMethod]
    public void RewriteDcpXml_adds_missing_copyright()
    {
        var doc = XDocument.Parse("<dcpData><ProfileName>Camera Standard</ProfileName></dcpData>");

        CameraProfileRewriter.RewriteDcpXml(doc, "Canon EOS R5", "Sony ILCE-7M4");

        Assert.AreEqual(CameraProfileRewriter.AuthorCredit, (string?)doc.Root!.Element("Copyright"));
        Assert.IsNull(doc.Root.Element("UniqueCameraModelRestriction"));
    }

    [TestMethod]
    public void RewriteXmp_rewrites_restriction_copyright_uuid_name_and_keeps_rest()
    {
        var path = TestFiles.Fixture("sample.xmp");
        var before = Hash(path);
        var original = XDocument.Load(path);
        var doc = new XDocument(original);
        var uuid = Guid.NewGuid();

        CameraProfileRewriter.RewriteXmp(doc, "Fujifilm X-T5", "BLEACH BYPASS", "Sony ILCE-7M4", uuid);

        var crs = CameraProfileRewriter.Crs;
        var rdf = CameraProfileRewriter.Rdf;
        var desc = doc.Descendants(rdf + "Description").Single();
        var oldDesc = original.Descendants(rdf + "Description").Single();
        Assert.AreEqual("Sony ILCE-7M4", (string?)desc.Attribute(crs + "CameraModelRestriction"));
        Assert.AreEqual(CameraProfileRewriter.AuthorCredit, (string?)desc.Attribute(crs + "Copyright"));
        Assert.AreEqual(uuid.ToString("N").ToUpperInvariant(), (string?)desc.Attribute(crs + "UUID"));
        Assert.AreNotEqual((string?)oldDesc.Attribute(crs + "UUID"), (string?)desc.Attribute(crs + "UUID"));
        Assert.AreEqual("Camera Standard", (string?)desc.Attribute(crs + "CameraProfile"));
        Assert.AreEqual("Fujifilm X-T5: BLEACH BYPASS", (string?)desc.Element(crs + "Name")!.Descendants(rdf + "li").Single());
        string[] touched = ["CameraModelRestriction", "Copyright", "UUID", "CameraProfile"];
        CollectionAssert.AreEqual(
            oldDesc.Attributes().Where(a => !touched.Contains(a.Name.LocalName)).Select(a => a.ToString()).ToList(),
            desc.Attributes().Where(a => !touched.Contains(a.Name.LocalName)).Select(a => a.ToString()).ToList());
        CollectionAssert.AreEqual(
            oldDesc.Elements().Skip(1).Select(e => e.ToString()).ToList(),
            desc.Elements().Skip(1).Select(e => e.ToString()).ToList());
        Assert.AreEqual(before, Hash(path));
    }
}
