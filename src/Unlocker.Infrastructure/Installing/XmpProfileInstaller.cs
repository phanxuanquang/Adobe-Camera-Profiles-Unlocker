using System.Xml.Linq;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Installing;

internal sealed class XmpProfileInstaller(AppPaths paths, CameraProfileRewriter rewriter, DcpProfileInstaller dcp)
{
    public static readonly XNamespace Crs = "http://ns.adobe.com/camera-raw-settings/1.0/";
    public static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    private static readonly string[] Attributes = ["Copyright", "CameraModelRestriction", "UUID", "CameraProfile"];

    /// <summary>Writes the XMP, plus a copy of the source's base DCP when the look needs it (shared by every look on that base).</summary>
    public async Task<IReadOnlyList<string>> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct)
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

        var sourceBase = tags.GetValueOrDefault("CameraProfile", "");
        var baseSource = Rewrite(tags, sourceModel, targetModel);

        foreach (var name in Attributes)
            if (tags.TryGetValue(name, out var value)) desc.SetAttributeValue(Crs + name, value);
        foreach (var li in names) li.Value = tags["Name"];

        List<string> files = [];
        string? newBase = null;
        if (baseSource is not null)
        {
            // Same output file as installing that DCP profile itself, so every look on this base shares one copy.
            // The catalog's name for that DCP: "Camera Standard" -> "Standard".
            var baseName = sourceBase.StartsWith("Camera ", StringComparison.Ordinal) ? sourceBase["Camera ".Length..] : sourceBase;
            var baseProfile = new CameraProfile(baseName, baseSource, CameraProfileFormat.DCP);
            var basePath = Path.Combine(paths.DcpOutputDir, rewriter.OutputFileName(sourceModel, baseProfile.Name, targetModel, CameraProfileFormat.DCP));
            var existed = File.Exists(basePath);
            await dcp.InstallAsync(baseProfile, sourceModel, targetModel, ct);
            if (!existed) newBase = basePath;
            files.Add(basePath);
        }

        Directory.CreateDirectory(paths.XmpOutputDir);
        var path = Path.Combine(paths.XmpOutputDir, rewriter.OutputFileName(sourceModel, profile.Name, targetModel, CameraProfileFormat.XMP));
        try
        {
            await using var write = File.Create(path);
            await doc.SaveAsync(write, SaveOptions.DisableFormatting, ct);
        }
        catch
        {
            File.Delete(path); // no half-written profile outside the manifest
            if (newBase is not null) File.Delete(newBase);
            throw;
        }
        files.Insert(0, path);
        return files;
    }

    /// <summary>Rewrites the tags. Returns the source's base DCP when the look must sit on a copy of it, else null.</summary>
    internal string? Rewrite(IDictionary<string, string> tags, string sourceModel, string targetModel)
    {
        rewriter.Rewrite(tags, sourceModel, targetModel);
        tags["UUID"] = Guid.NewGuid().ToString("N").ToUpperInvariant();
        // A profile without a base (e.g. RICOH GR IV Monochrome) stays without one.
        if (!tags.TryGetValue("CameraProfile", out var sourceBase)) return null;
        var (baseName, baseSource) = BaseFor(sourceBase, sourceModel, targetModel);
        tags["CameraProfile"] = baseName;
        return baseSource;
    }

    /// <summary>
    /// The look sits on a base profile Camera Raw finds by name for the target camera, first match wins:
    /// the same base when the target has it (same brand only: "Camera Standard" of another brand is another look);
    /// else a copy of the source's base DCP; else the target's Camera Standard (v2 first) or Adobe Standard.
    /// </summary>
    private (string Name, string? Source) BaseFor(string sourceBase, string sourceModel, string targetModel)
    {
        if (sourceBase == "Adobe Standard") return (sourceBase, null);
        var sameBrand = string.Equals(sourceModel.Split(' ', 2)[0], targetModel.Split(' ', 2)[0], StringComparison.OrdinalIgnoreCase);
        // shortcut: assumes every model of the brand has the built-in base; check per model if a copy shows no base.
        if (sameBrand && (ProfileFile(targetModel, sourceBase) is not null || BuiltInBases.Contains(sourceBase))) return (sourceBase, null);
        if (ProfileFile(sourceModel, sourceBase) is { } file)
        {
            var name = new Dictionary<string, string> { ["Name"] = sourceBase };
            rewriter.Rewrite(name, sourceModel, targetModel); // the name the copied DCP gets: "Nikon Z f: Standard"
            return (name["Name"], file);
        }
        string[] fallbacks = ["Camera Standard v2", "Camera Standard"];
        return (fallbacks.FirstOrDefault(b => ProfileFile(targetModel, b) is not null) ?? "Adobe Standard", null);
    }

    /// <summary>
    /// Bases built into Camera Raw, with no DCP on disk (the name is in CameraRaw.dll). Other missing bases are not trusted:
    /// Adobe's Nikon D780 XMPs name "Camera Standard v2", which exists neither as a file nor in the DLL.
    /// </summary>
    private static readonly string[] BuiltInBases = ["Camera PROVIA/Standard"];

    /// <summary>Some files use '_' for spaces ("Leica D-Lux 7 Camera_Standard_v2.dcp"); the profile name inside keeps the spaces.</summary>
    private string? ProfileFile(string model, string name)
        => new[] { paths.CameraProfileFile(model, name), paths.CameraProfileFile(model, name.Replace(' ', '_')) }.FirstOrDefault(File.Exists);
}
