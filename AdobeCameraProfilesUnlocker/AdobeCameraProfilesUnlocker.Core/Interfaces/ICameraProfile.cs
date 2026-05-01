using AdobeCameraProfilesUnlocker.Core.Models;
using AdobeCameraProfilesUnlocker.Core.Models.Enums;

namespace AdobeCameraProfilesUnlocker.Core.Interfaces
{
    public interface ICameraProfile
    {
        Task<List<CameraProfile>> ConvertAsync(List<CameraProfile> profiles, CameraProfileExtension targetType, string outputDirectory);
        Task OverrideCameraRestrictionAsync(List<CameraProfile> profiles, Camera targetCamera);
    }
}
