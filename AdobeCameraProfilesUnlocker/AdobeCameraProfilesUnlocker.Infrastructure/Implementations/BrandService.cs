using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using System.Collections.Frozen;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class BrandService : IBrandService
{
    public Task<FrozenDictionary<Guid, string>> GetAllBrandsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<FrozenDictionary<Guid, string>> GetCameraModelsByBrandIdAsync(Guid brandId)
    {
        throw new NotImplementedException();
    }

    public Task<FrozenDictionary<Guid, string>> GetProfilesByBrandIdAsync(Guid brandId)
    {
        throw new NotImplementedException();
    }
}