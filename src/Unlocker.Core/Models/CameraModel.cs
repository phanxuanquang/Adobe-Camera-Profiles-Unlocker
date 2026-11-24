namespace Unlocker.Core.Models;

public sealed record CameraModel(string Name, IReadOnlyList<CameraProfile> Profiles);
