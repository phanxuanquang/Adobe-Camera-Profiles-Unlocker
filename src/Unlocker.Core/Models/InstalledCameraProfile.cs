namespace Unlocker.Core.Models;

/// <param name="Name">Camera profile name, e.g. "CLASSIC CHROME".</param>
/// <param name="Files">Absolute paths this app created.</param>
/// <param name="IsLegacy">Written by the old admin app. Deleting needs elevation.</param>
public sealed record InstalledCameraProfile(
    string Name,
    string SourceModel,
    string TargetModel,
    IReadOnlyList<string> Files,
    bool IsLegacy = false);
