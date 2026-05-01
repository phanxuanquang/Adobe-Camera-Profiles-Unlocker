using AdobeCameraProfilesUnlocker.Domain.Models;

namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface ICameraProfileService
{
    Task<CameraProfile[]> GetCameraProfilesByBrandIdAsync(Guid brandId);
    Task<CameraProfile[]> GetCameraProfileByCameraIdAsync(Guid cameraId);
    Task<CameraProfile> GetCameraProfileByIdAsync(Guid id);
    Task<CameraProfile[]> GetCameraProfilesByIdsAsync(Guid[] ids);
    Task AdaptToCameraModelAsync(CameraProfile[] profiles, Guid cameraModelId);
}