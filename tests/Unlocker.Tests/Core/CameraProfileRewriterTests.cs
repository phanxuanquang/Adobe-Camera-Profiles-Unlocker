using Unlocker.Core.Enums;
using Unlocker.Core.Rules;

namespace Unlocker.Tests.Core;

[TestClass]
public sealed class CameraProfileRewriterTests
{
    [TestMethod]
    public void Author_credit_and_signature_come_from_build_props()
    {
        Assert.AreEqual("© 2024 Phan Xuan Quang / Github: @phanxuanquang", CameraProfileRewriter.AuthorCredit);
        Assert.AreEqual("Phan Xuan Quang", CameraProfileRewriter.AuthorSignature);
    }

    [TestMethod]
    [DataRow(CameraProfileFormat.DCP, "Portrait (for Sony ILCE-7M4).dcp")]
    [DataRow(CameraProfileFormat.XMP, "Portrait (for Sony ILCE-7M4).xmp")]
    public void OutputFileName_uses_format_extension(CameraProfileFormat format, string expected)
        => Assert.AreEqual(expected, new CameraProfileRewriter().OutputFileName("Portrait", "Sony ILCE-7M4", format));

    [TestMethod]
    public void OutputFileName_replaces_invalid_chars()
        => Assert.AreEqual("PROVIA-Standard (for Sony ILCE-7M4).xmp",
            new CameraProfileRewriter().OutputFileName("PROVIA/Standard", "Sony ILCE-7M4", CameraProfileFormat.XMP));

    [TestMethod]
    public void Rewrite_prefixes_name_sets_copyright_and_restriction()
    {
        var tags = new Dictionary<string, string>
        {
            ["Name"] = "Camera Portrait",
            ["Copyright"] = "Copyright Adobe",
            ["CameraModelRestriction"] = "Canon EOS R5",
            ["Other"] = "kept",
        };

        new CameraProfileRewriter().Rewrite(tags, "Canon EOS R5", "Sony ILCE-7M4");

        Assert.AreEqual("Canon EOS R5: Portrait", tags["Name"]);
        Assert.AreEqual(CameraProfileRewriter.AuthorCredit, tags["Copyright"]);
        Assert.AreEqual("Sony ILCE-7M4", tags["CameraModelRestriction"]);
        Assert.AreEqual("kept", tags["Other"]);
    }

    [TestMethod]
    public void Rewrite_adds_missing_copyright_only()
    {
        var tags = new Dictionary<string, string> { ["Name"] = "Camera Standard" };

        new CameraProfileRewriter().Rewrite(tags, "Canon EOS R5", "Sony ILCE-7M4");

        CollectionAssert.AreEquivalent(new[] { "Name", "Copyright" }, tags.Keys.ToArray());
    }
}
