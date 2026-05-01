using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class CameraProfileService : ICameraProfileService
{
    public Task AdaptToCameraModelAsync(CameraProfile[] profiles, Guid cameraModelId)
    {
        throw new NotImplementedException();
    }

    public Task<CameraProfile> GetCameraProfileByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<CameraProfile[]> GetCameraProfilesByBrandIdAsync(Guid brandId)
    {
        throw new NotImplementedException();
    }

    public Task<CameraProfile[]> GetCameraProfilesByIdsAsync(Guid[] ids)
    {
        throw new NotImplementedException();
    }
}