using System.Collections.Frozen;

namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface IBrandService
{
    Task<FrozenDictionary<Guid, string>> GetAllBrandsAsync();
    Task<FrozenDictionary<Guid, string>> GetCameraModelsByBrandIdAsync(Guid brandId);
    Task<FrozenDictionary<Guid, string>> GetProfilesByBrandIdAsync(Guid brandId);
}