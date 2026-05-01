namespace AdobeCameraProfilesUnlocker.Infrastructure.Helpers;

internal sealed class ProfileKeyComparer : IEqualityComparer<(string Name, Guid CameraId)>
{
    public static readonly ProfileKeyComparer Instance = new();

    public bool Equals((string Name, Guid CameraId) x, (string Name, Guid CameraId) y)
        => x.CameraId == y.CameraId && string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode((string Name, Guid CameraId) obj)
        => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name), obj.CameraId);
}