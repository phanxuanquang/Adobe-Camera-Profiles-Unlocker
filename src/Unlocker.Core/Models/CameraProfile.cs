namespace Unlocker.Core.Models;

/// <param name="Name">Name without the model prefix, e.g. "CLASSIC CHROME".</param>
/// <param name="SourcePath">Adobe original. Read-only.</param>
public sealed record CameraProfile(string Name, string SourcePath, CameraProfileFormat Format);
