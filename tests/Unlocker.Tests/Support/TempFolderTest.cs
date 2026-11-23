using System.Xml.Linq;
using Unlocker.Infrastructure.Installing;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Tests.Support;

/// <summary>Base for tests that touch the file system. Paths point into a temp folder, never the real Camera Raw folders.</summary>
public abstract class TempFolderTest
{
    protected string Root { get; private set; } = null!;

    internal AppPaths Paths { get; private set; } = null!;

    [TestInitialize]
    public void CreateTempFolder()
    {
        Root = Directory.CreateTempSubdirectory("unlocker-tests-").FullName;
        Paths = AppPaths.Create(Path.Combine(Root, "ProgramData"), Path.Combine(Root, "LocalAppData"));
    }

    [TestCleanup]
    public void DeleteTempFolder() => Directory.Delete(Root, recursive: true);

    protected static string Touch(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "x");
        return path;
    }

    /// <summary>Writes the sample XMP restricted to <paramref name="model"/> under the Adobe XMP folder.</summary>
    protected string WriteXmp(string relativePath, string model)
    {
        var doc = XDocument.Load(TestFiles.Fixture("sample.xmp"));
        var desc = doc.Descendants(XmpProfileInstaller.Rdf + "Description").Single();
        desc.SetAttributeValue(XmpProfileInstaller.Crs + "CameraModelRestriction", model);
        var path = Path.Combine(Paths.XmpProfilesDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        doc.Save(path);
        return path;
    }
}
