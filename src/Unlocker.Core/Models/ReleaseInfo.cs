namespace Unlocker.Core.Models;

/// <summary>PageUrl is the release page. Users download from there themselves.</summary>
public sealed record ReleaseInfo(Version Version, Uri PageUrl);
