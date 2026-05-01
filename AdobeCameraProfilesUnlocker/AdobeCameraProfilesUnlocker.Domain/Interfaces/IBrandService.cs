namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

public interface IBrandService
{
    Task<Dictionary<Guid, string>> GetAllBrandsAsync();
    Task<Dictionary<Guid, string>> GetCameraModelsByBrandIdAsync(Guid brandId);
    Task<Dictionary<Guid, string>> GetProfilesByBrandIdAsync(Guid brandId);
}