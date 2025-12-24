using AdobeCameraProfilesUnlocker.Core.Models;
using AdobeCameraProfilesUnlocker.Core.Models.Enums;

namespace AdobeCameraProfilesUnlocker.Core.Extensions
{
    public static class CameraExtensions
    {
        public static string[] GetProfilePaths(this Camera camera)
        {
            switch (camera.ProfileType)
            {
                case CameraProfileType.DCP:
                    var dcpProfilesDirectory = Path.Combine(camera.ProfileType.GetRootDirectory(), camera.Name);
                    return Directory.GetFiles(
                        dcpProfilesDirectory,
                        $"*.{camera.ProfileType.ToString().ToLower()}",
                        SearchOption.TopDirectoryOnly);
                case CameraProfileType.XMP:
                    var cameraBrand = camera.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                    var xmpProfilesDirectory = Path.Combine(camera.ProfileType.GetRootDirectory(), cameraBrand, camera.Name);
                    return Directory.GetFiles(
                        xmpProfilesDirectory,
                        $"*.{camera.ProfileType.ToString().ToLower()}",
                        SearchOption.TopDirectoryOnly);
                default:
                    throw new NotSupportedException($"The camera profile type '{camera.ProfileType}' is not supported.");
            }
        }

        public static Task<string[]> GetProfilePathsAsync(this Camera camera)
        {
            return Task.Run(() => camera.GetProfilePaths());
        }
    }
}
