using AdobeCameraProfilesUnlocker.Domain.Models;

namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface ICameraProfileService
{
    Task<CameraProfile[]> GetCameraProfilesByBrandIdAsync(Guid brandId);
    Task<CameraProfile[]> GetCameraProfileByCameraIdAsync(Guid cameraId);
    Task<CameraProfile> GetCameraProfileByIdAsync(Guid id);
    Task<CameraProfile[]> GetCameraProfilesByIdsAsync(IEnumerable<Guid> ids);
}