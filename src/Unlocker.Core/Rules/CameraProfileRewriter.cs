using System.Xml.Linq;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;

namespace Unlocker.Core.Rules;

/// <summary>Rewrite rules for copied camera profiles (docs/profile-format.md). XDocument only, never text replace.</summary>
public static class CameraProfileRewriter
{
    public const string AuthorCredit = "© 2024 Phan Xuan Quang / Github: @phanxuanquang";

    public const string AuthorSignature = "Phan Xuan Quang";

    public static readonly XNamespace Crs = "http://ns.adobe.com/camera-raw-settings/1.0/";
    public static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    /// <summary>Rewrites XML produced by `dcpTool -d`. Adds a missing Copyright and skips other missing elements.</summary>
    public static void RewriteDcpXml(XDocument doc, string sourceModel, string targetModel)
    {
        var root = doc.Root ?? throw new ArgumentException("Empty DCP XML.", nameof(doc));
        if (root.Element("ProfileName") is { } name)
            name.Value = PrefixedName(sourceModel, name.Value);
        if (root.Element("Copyright") is { } copyright) copyright.Value = AuthorCredit;
        else root.Add(new XElement("Copyright", AuthorCredit));
        if (root.Element("ProfileCalibrationSignature") is { } signature)
            signature.Value = signature.Value.Replace("com.adobe", AuthorSignature, StringComparison.Ordinal);
        if (root.Element("UniqueCameraModelRestriction") is { } restriction) restriction.Value = targetModel;
    }

    public static void RewriteXmp(XDocument doc, string sourceModel, string profileName, string targetModel, Guid newUuid)
    {
        var desc = doc.Descendants(Rdf + "Description").FirstOrDefault()
            ?? throw new ArgumentException("XMP has no rdf:Description.", nameof(doc));
        desc.SetAttributeValue(Crs + "CameraModelRestriction", targetModel);
        desc.SetAttributeValue(Crs + "Copyright", AuthorCredit);
        desc.SetAttributeValue(Crs + "UUID", newUuid.ToString("N").ToUpperInvariant());
        desc.SetAttributeValue(Crs + "CameraProfile", "Camera Standard");
        var name = desc.Element(Crs + "Name");
        if (name is null) desc.Add(name = new XElement(Crs + "Name", new XElement(Rdf + "Alt", new XElement(Rdf + "li"))));
        foreach (var li in name.Descendants(Rdf + "li")) li.Value = $"{sourceModel}: {profileName}";
    }

    /// <summary>"Camera Portrait" -> "Canon EOS R5: Portrait".</summary>
    private static string PrefixedName(string sourceModel, string profileName) =>
        profileName.StartsWith("Camera ", StringComparison.Ordinal)
            ? $"{sourceModel}: {profileName["Camera ".Length..]}"
            : $"{sourceModel}: {profileName}";

    /// <summary>"{name} (for {target}).dcp|.xmp" with invalid file-name chars replaced by '-'.</summary>
    public static string OutputFileName(string profileName, string targetModel, CameraProfileFormat format)
    {
        var name = $"{profileName} (for {targetModel}){(format == CameraProfileFormat.DCP ? ".dcp" : ".xmp")}";
        return string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '-' : c));
    }

    /// <summary>"Canon EOS R5 Camera Portrait.dcp" + "Canon EOS R5" -> "Portrait". Null when the pattern does not match.</summary>
    public static string? ParseProfileName(string fileName, string model)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var prefix = model + " Camera ";
        return stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && stem.Length > prefix.Length
            ? stem[prefix.Length..]
            : null;
    }
}
