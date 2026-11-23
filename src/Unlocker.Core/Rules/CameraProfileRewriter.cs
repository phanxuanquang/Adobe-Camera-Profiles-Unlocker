using System.Reflection;
using Unlocker.Core.Enums;

namespace Unlocker.Core.Rules;

public sealed class CameraProfileRewriter
{
    private static readonly Assembly Self = typeof(CameraProfileRewriter).Assembly;
    public static string AuthorCredit { get; } = Self.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
        ?? throw new InvalidOperationException("Copyright missing in Directory.Build.props.");

    public static string AuthorSignature { get; } = Self.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(a => a.Key == "Author")?.Value
        ?? throw new InvalidOperationException("Authors missing in Directory.Build.props.");

    public void Rewrite(IDictionary<string, string> tags, string sourceModel, string targetModel)
    {
        if (tags.TryGetValue("Name", out var name))
            tags["Name"] = $"{sourceModel}: {(name.StartsWith("Camera ", StringComparison.Ordinal) ? name["Camera ".Length..] : name)}";
        tags["Copyright"] = AuthorCredit;
        if (tags.ContainsKey("CameraModelRestriction")) tags["CameraModelRestriction"] = targetModel;
    }

    public string OutputFileName(string profileName, string targetModel, CameraProfileFormat format)
    {
        var name = $"{profileName} (for {targetModel}).{format.ToString().ToLowerInvariant()}";
        return string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '-' : c));
    }
}
