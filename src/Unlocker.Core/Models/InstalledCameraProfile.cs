namespace Unlocker.Core.Models;

/// <param name="Name">Camera profile name, e.g. "CLASSIC CHROME".</param>
/// <param name="Files">Absolute paths this app created.</param>
public sealed record InstalledCameraProfile(
    string Name,
    string SourceModel,
    string TargetModel,
    IReadOnlyList<string> Files);
