namespace Unlocker.Core.Models;

public sealed record CameraModel(string Name, string Brand, IReadOnlyList<CameraProfile> Profiles);
