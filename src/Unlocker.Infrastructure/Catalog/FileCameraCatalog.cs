using System.Xml;
using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Infrastructure.Installing;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Catalog;

/// <summary>Scans Camera Raw once and caches names and paths only (layout: docs/adobe-paths.md).</summary>
internal sealed class FileCameraCatalog(AppPaths paths, ILogger<FileCameraCatalog> logger) : ICameraCatalog
{
    private static readonly string RdfNs = XmpProfileInstaller.Rdf.NamespaceName;
    private static readonly string CrsNs = XmpProfileInstaller.Crs.NamespaceName;

    /// <summary>What each brand calls its camera profiles.</summary>
    private static readonly Dictionary<string, string> Families = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fujifilm"] = "Film Simulations",
        ["Canon"] = "Picture Styles",
        ["Nikon"] = "Picture Controls",
        ["Sony"] = "Creative Looks",
        ["Panasonic"] = "Photo Styles",
        ["Leica"] = "Film Styles",
        ["Olympus"] = "Picture Modes",
        ["OM"] = "Picture Modes",
        ["Pentax"] = "Custom Images",
        ["Ricoh"] = "Custom Images",
        ["Sigma"] = "Color Modes",
    };

    private readonly Lazy<Task<IReadOnlyList<Brand>>> _cache = new(() => Task.Run(() => Scan(paths, logger)));

    public Task<IReadOnlyList<Brand>> LoadAsync(CancellationToken ct = default) => _cache.Value.WaitAsync(ct);

    private static IReadOnlyList<Brand> Scan(AppPaths paths, ILogger logger)
    {
        var models = new Dictionary<string, List<CameraProfile>>(StringComparer.OrdinalIgnoreCase);

        // DCP: <dir>\<Model>\<Model> Camera <Name>.dcp. The folder name is the model.
        if (Directory.Exists(paths.DcpProfilesDir))
        {
            foreach (var dir in Directory.EnumerateDirectories(paths.DcpProfilesDir))
            {
                var model = Path.GetFileName(dir);
                foreach (var file in Directory.EnumerateFiles(dir, "*.dcp"))
                {
                    if (ParseProfileName(file, model) is not { } name) continue;
                    Add(models, model, new CameraProfile(name, file, CameraProfileFormat.DCP));
                }
            }
        }

        // XMP: <dir>\<Brand>[\<Model>]\<Model> Camera <Name>.xmp. The folder can be brand or model,
        // so the model comes from crs:CameraModelRestriction. XMP wins over DCP on a duplicate name.
        if (Directory.Exists(paths.XmpProfilesDir))
        {
            foreach (var file in Directory.EnumerateFiles(paths.XmpProfilesDir, "*.xmp", SearchOption.AllDirectories))
            {
                if (ReadXmpModel(file, logger) is not { } model) continue;
                if (ParseProfileName(file, model) is not { } name) continue;
                if (models.TryGetValue(model, out var existing))
                    existing.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                Add(models, model, new CameraProfile(name, file, CameraProfileFormat.XMP));
            }
        }

        // ponytail: brand = first word of the model ("OM Digital Solutions OM-1" -> "OM"). Add an alias map if a brand groups wrong.
        var brands = models
            .Select(m => new CameraModel(m.Key, m.Key.Split(' ', 2)[0], m.Value.OrderBy(p => p.Name).ToList()))
            .GroupBy(m => m.Brand, StringComparer.OrdinalIgnoreCase)
            .Select(g => new Brand(g.Key, Families.GetValueOrDefault(g.Key, "Camera Profiles"), g.OrderBy(m => m.Name).ToList()))
            .OrderBy(b => b.Name)
            .ToList();
        logger.LogInformation("Camera catalog scanned: {BrandCount} brands, {ModelCount} models, {ProfileCount} profiles",
            brands.Count, models.Count, models.Sum(m => m.Value.Count));
        return brands;
    }

    /// <summary>Reads only the first rdf:Description start tag: the files embed large RGB tables.</summary>
    private static string? ReadXmpModel(string file, ILogger logger)
    {
        try
        {
            using var reader = XmlReader.Create(file, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            if (!reader.ReadToFollowing("Description", RdfNs)) return null;
            var model = reader.GetAttribute("CameraModelRestriction", CrsNs);
            return string.IsNullOrWhiteSpace(model) ? null : model;
        }
        catch (Exception e) when (e is XmlException or IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(e, "Skipped unreadable XMP profile {File}", file);
            return null;
        }
    }

    /// <summary>"Canon EOS R5 Camera Portrait.dcp" + "Canon EOS R5" -> "Portrait". Null when the pattern does not match.</summary>
    private static string? ParseProfileName(string fileName, string model)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var prefix = model + " Camera ";
        return stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && stem.Length > prefix.Length
            ? stem[prefix.Length..]
            : null;
    }

    private static void Add(Dictionary<string, List<CameraProfile>> models, string model, CameraProfile profile)
    {
        if (!models.TryGetValue(model, out var list)) models[model] = list = [];
        list.Add(profile);
    }
}
