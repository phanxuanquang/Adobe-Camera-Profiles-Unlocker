using System.Xml.Linq;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Installing;

internal sealed class XmpProfileInstaller(AppPaths paths, CameraProfileRewriter rewriter)
{
    public static readonly XNamespace Crs = "http://ns.adobe.com/camera-raw-settings/1.0/";
    public static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    private static readonly string[] Attributes = ["Copyright", "CameraModelRestriction", "UUID", "CameraProfile"];

    public async Task<string> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct)
    {
        XDocument doc;
        await using (var read = File.OpenRead(profile.SourcePath))
            doc = await XDocument.LoadAsync(read, LoadOptions.PreserveWhitespace, ct);
        var desc = doc.Descendants(Rdf + "Description").FirstOrDefault()
            ?? throw new InvalidDataException("XMP has no rdf:Description.");

        var tags = new Dictionary<string, string>();
        foreach (var name in Attributes)
            if (desc.Attribute(Crs + name) is { } attribute) tags[name] = attribute.Value;
        // Adobe XMPs carry one x-default entry; every language entry gets the same name.
        var names = desc.Element(Crs + "Name")?.Descendants(Rdf + "li").ToList() ?? [];
        if (names.Count > 0) tags["Name"] = names[0].Value;

        Rewrite(tags, sourceModel, targetModel);

        foreach (var name in Attributes)
            if (tags.TryGetValue(name, out var value)) desc.SetAttributeValue(Crs + name, value);
        foreach (var li in names) li.Value = tags["Name"];

        Directory.CreateDirectory(paths.XmpOutputDir);
        var path = Path.Combine(paths.XmpOutputDir, rewriter.OutputFileName(profile.Name, targetModel, CameraProfileFormat.XMP));
        await using (var write = File.Create(path))
            await doc.SaveAsync(write, SaveOptions.DisableFormatting, ct);
        return path;
    }

    internal void Rewrite(IDictionary<string, string> tags, string sourceModel, string targetModel)
    {
        rewriter.Rewrite(tags, sourceModel, targetModel);
        tags["UUID"] = Guid.NewGuid().ToString("N").ToUpperInvariant();
        tags["CameraProfile"] = "Camera Standard";
    }
}
