using Microsoft.Extensions.Logging.Abstractions;
using System.Xml.Linq;
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
        Dcp(),
        new XmpProfileInstaller(Paths, new CameraProfileRewriter(), Dcp()),
        NullLogger<CameraProfileInstaller>.Instance);

    private DcpProfileInstaller Dcp() => new(Paths, new CameraProfileRewriter(), NullLogger<DcpProfileInstaller>.Instance);

    [TestMethod]
    public async Task Dcp_without_adobe_standard_writes_rewritten_copy_to_output_dir()
    {
        var src = Path.Combine(Paths.DcpProfilesDir, "Canon EOS R5", "Canon EOS R5 Camera Portrait.dcp");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        var before = DcpFileTests.Sample();
        File.WriteAllBytes(src, before);
        var profile = new CameraProfile("Portrait", src, CameraProfileFormat.DCP);

        var result = await Installer().InstallAsync(profile, "Canon EOS R5", "Sony ILCE-7M4");

        var output = Path.Combine(Paths.DcpOutputDir, "Canon EOS R5 Portrait (for Sony ILCE-7M4).dcp");
        CollectionAssert.AreEqual(new[] { output }, result.Files.ToArray());
        CollectionAssert.AreEqual(DcpFileTests.Rewrite(before, t => Dcp().Rewrite(t, "Canon EOS R5", "Sony ILCE-7M4")), File.ReadAllBytes(output));
        var strings = DcpFileTests.ReadStrings(File.ReadAllBytes(output));
        Assert.AreEqual("Sony ILCE-7M4", strings["CameraModelRestriction"]);
        Assert.AreEqual("Canon EOS R5: Portrait", strings["Name"]);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(src));
        Assert.AreEqual(("Portrait", "Canon EOS R5", "Sony ILCE-7M4"), (result.Name, result.SourceModel, result.TargetModel));
    }

    [TestMethod]
    public async Task Dcp_takes_calibration_from_target_adobe_standard_and_keeps_source_look()
    {
        var src = Path.Combine(Paths.DcpProfilesDir, "Canon EOS R5", "Canon EOS R5 Camera Portrait.dcp");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        File.WriteAllBytes(src, DcpFileTests.Sample(matrixSeed: 0, lookSeed: 1));
        var adobeStandard = DcpFileTests.Sample("Adobe Standard", "Sony ILCE-7M4", matrixSeed: 100, lookSeed: 2);
        Directory.CreateDirectory(Paths.AdobeStandardDir);
        File.WriteAllBytes(Paths.AdobeStandardFile("Sony ILCE-7M4"), adobeStandard);

        var result = await Installer().InstallAsync(new CameraProfile("Portrait", src, CameraProfileFormat.DCP), "Canon EOS R5", "Sony ILCE-7M4");

        var output = DcpFile.Parse(File.ReadAllBytes(result.Files[0])).Entries;
        byte[] Data(IReadOnlyList<DcpFile.Entry> entries, ushort tag) => entries.Single(e => e.Tag == tag).Data.ToArray();
        CollectionAssert.AreEqual(Data(DcpFile.Parse(adobeStandard).Entries, 0xC621), Data(output, 0xC621)); // ColorMatrix1 from Adobe Standard
        CollectionAssert.AreEqual(Data(DcpFile.Parse(File.ReadAllBytes(src)).Entries, 0xC726), Data(output, 0xC726)); // look table from source
        Assert.AreEqual("Canon EOS R5: Portrait", DcpFileTests.ReadStrings(File.ReadAllBytes(result.Files[0]))["Name"]);
    }

    [TestMethod]
    public async Task Dcp_prefers_adobe_standard_v2()
    {
        var src = WriteSourceDcp();
        Directory.CreateDirectory(Paths.AdobeStandardDir);
        File.WriteAllBytes(Paths.AdobeStandardFile("Sony ILCE-7M4"), DcpFileTests.Sample("Adobe Standard", "Sony ILCE-7M4", matrixSeed: 100));
        var v2 = DcpFileTests.Sample("Adobe Standard v2", "Sony ILCE-7M4", matrixSeed: 200);
        File.WriteAllBytes(Path.Combine(Paths.AdobeStandardDir, "Sony ILCE-7M4 Adobe Standard v2.dcp"), v2);

        var result = await Installer().InstallAsync(new CameraProfile("Portrait", src, CameraProfileFormat.DCP), "Canon EOS R5", "Sony ILCE-7M4");

        CollectionAssert.AreEqual(ColorMatrix1(v2), ColorMatrix1(File.ReadAllBytes(result.Files[0])));
    }

    [TestMethod]
    public async Task Dcp_finds_adobe_standard_by_camera_model_when_file_name_differs()
    {
        var src = WriteSourceDcp();
        Directory.CreateDirectory(Paths.AdobeStandardDir);
        var adobeStandard = DcpFileTests.Sample("Adobe Standard", "Leica D-Lux 7", matrixSeed: 100);
        File.WriteAllBytes(Path.Combine(Paths.AdobeStandardDir, "Leica D-Lux 7 Adobe_Standard.dcp"), adobeStandard);
        File.WriteAllBytes(Path.Combine(Paths.AdobeStandardDir, "Other Adobe Standard.dcp"), DcpFileTests.Sample("Adobe Standard", "Other", matrixSeed: 50));

        var result = await Installer().InstallAsync(new CameraProfile("Portrait", src, CameraProfileFormat.DCP), "Canon EOS R5", "Leica D-Lux 7");

        CollectionAssert.AreEqual(ColorMatrix1(adobeStandard), ColorMatrix1(File.ReadAllBytes(result.Files[0])));
    }

    private string WriteSourceDcp()
    {
        var src = Path.Combine(Paths.DcpProfilesDir, "Canon EOS R5", "Canon EOS R5 Camera Portrait.dcp");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        File.WriteAllBytes(src, DcpFileTests.Sample());
        return src;
    }

    private static byte[] ColorMatrix1(byte[] dcp) => DcpFile.Parse(dcp).Entries.Single(e => e.Tag == 0xC621).Data.ToArray();

    [TestMethod]
    public async Task Dcp_with_unreadable_adobe_standard_keeps_source_calibration()
    {
        var src = Path.Combine(Paths.DcpProfilesDir, "Canon EOS R5", "Canon EOS R5 Camera Portrait.dcp");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        var before = DcpFileTests.Sample();
        File.WriteAllBytes(src, before);
        Touch(Paths.AdobeStandardFile("Sony ILCE-7M4")); // "x": not a DCP

        var result = await Installer().InstallAsync(new CameraProfile("Portrait", src, CameraProfileFormat.DCP), "Canon EOS R5", "Sony ILCE-7M4");

        CollectionAssert.AreEqual(DcpFileTests.Rewrite(before, t => Dcp().Rewrite(t, "Canon EOS R5", "Sony ILCE-7M4")), File.ReadAllBytes(result.Files[0]));
    }

    [TestMethod]
    public async Task Xmp_writes_rewritten_copy_and_leaves_source_unchanged()
    {
        var src = WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");
        var before = File.ReadAllBytes(src);
        var profile = new CameraProfile("CLASSIC Neg", src, CameraProfileFormat.XMP);

        var result = await Installer().InstallAsync(profile, "Fujifilm X-T5", "Sony ILCE-7M4");

        var output = Path.Combine(Paths.XmpOutputDir, "Fujifilm X-T5 CLASSIC Neg (for Sony ILCE-7M4).xmp");
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

    [TestMethod]
    public async Task Xmp_across_brands_installs_a_shared_copy_of_the_source_base()
    {
        File.WriteAllBytes(Touch(Paths.CameraProfileFile("Nikon Z f", "Camera Standard")), DcpFileTests.Sample("Camera Standard", "Nikon Z f"));
        Touch(Paths.CameraProfileFile("Canon EOS R6", "Camera Standard"));
        CameraProfile Look(string name)
        {
            var src = WriteXmp($@"Nikon\Nikon Z f\Nikon Z f Camera {name}.xmp", "Nikon Z f");
            var doc = XDocument.Load(src);
            doc.Descendants(XmpProfileInstaller.Rdf + "Description").Single().SetAttributeValue(XmpProfileInstaller.Crs + "CameraProfile", "Camera Standard");
            doc.Save(src);
            return new CameraProfile(name, src, CameraProfileFormat.XMP);
        }

        var bleach = await Installer().InstallAsync(Look("Bleach"), "Nikon Z f", "Canon EOS R6");
        var toy = await Installer().InstallAsync(Look("Toy"), "Nikon Z f", "Canon EOS R6");

        var basePath = Path.Combine(Paths.DcpOutputDir, "Nikon Z f Standard (for Canon EOS R6).dcp");
        CollectionAssert.AreEqual(new[] { Path.Combine(Paths.XmpOutputDir, "Nikon Z f Bleach (for Canon EOS R6).xmp"), basePath }, bleach.Files.ToArray());
        Assert.AreEqual(basePath, toy.Files[1]);
        var baseStrings = DcpFileTests.ReadStrings(File.ReadAllBytes(basePath));
        Assert.AreEqual(("Nikon Z f: Standard", "Canon EOS R6"), (baseStrings["Name"], baseStrings["CameraModelRestriction"]));
        Assert.AreEqual("Nikon Z f: Standard",
            (string?)XDocument.Load(bleach.Files[0]).Descendants(XmpProfileInstaller.Rdf + "Description").Single().Attribute(XmpProfileInstaller.Crs + "CameraProfile"));
    }

    [TestMethod]
    public async Task Xmp_failing_mid_write_leaves_no_partial_file()
    {
        var src = WriteXmp(@"Fujifilm\Fujifilm X-T5\Fujifilm X-T5 Camera CLASSIC Neg.xmp", "Fujifilm X-T5");

        // U+0001 is not valid XML: the writer throws after the output file was created.
        await Assert.ThrowsAsync<ArgumentException>(() => Installer().InstallAsync(
            new CameraProfile("CLASSIC Neg", src, CameraProfileFormat.XMP), "Fujifilm\u0001X-T5", "Sony ILCE-7M4"));

        Assert.AreEqual(0, Directory.EnumerateFiles(Paths.XmpOutputDir).Count());
    }
}
