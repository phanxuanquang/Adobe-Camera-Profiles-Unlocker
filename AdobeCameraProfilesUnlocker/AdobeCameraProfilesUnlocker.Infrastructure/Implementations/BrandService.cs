using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class BrandService : IBrandService
{
    private readonly AppDbContext _db;
    private readonly ILogger<BrandService> _logger;

    public BrandService(AppDbContext db, ILoggerFactory? loggerFactory = null)
    {
        _db = db;
        _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<BrandService>();
    }

    public async Task<Dictionary<Guid, string>> GetAllBrandsAsync()
    {
        _logger.LogTrace("Getting all camera brands from database...");
        return await _db.Brands
            .AsNoTracking()
            .ToDictionaryAsync(b => b.Id, b => b.Name);
    }

    public async Task<Dictionary<Guid, string>> GetCameraModelsByBrandIdAsync(Guid brandId)
    {
        _logger.LogTrace("Getting camera models for brand id {brandId} from database...", brandId);
        return await _db.Cameras
            .AsNoTracking()
            .Where(cm => cm.BrandId == brandId)
            .ToDictionaryAsync(cm => cm.Id, cm => cm.CodeName);
    }

    public async Task<Dictionary<Guid, string>> GetProfilesByBrandIdAsync(Guid brandId)
    {
        _logger.LogTrace("Getting camera profiles for brand id {brandId} from database...", brandId);
        return await _db.Profiles
            .AsNoTracking()
            .Where(cp => cp.BrandId == brandId)
            .ToDictionaryAsync(cp => cp.Id, cp => cp.Name);
    }
}