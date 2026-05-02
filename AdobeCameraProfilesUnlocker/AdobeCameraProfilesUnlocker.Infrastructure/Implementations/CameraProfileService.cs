using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class CameraProfileService(AppDbContext db, ILoggerFactory? loggerFactory = null) : ICameraProfileService
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<CameraProfileService> _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CameraProfileService>();

    public async Task<CameraProfile[]> GetCameraProfileByCameraIdAsync(Guid cameraId)
    {
        _logger.LogTrace("Retrieving camera profiles for camera model with ID {CameraModelId}", cameraId);
        return await _db.Profiles
            .AsNoTracking()
            .Where(p => p.CameraId == cameraId)
            .OrderBy(p => p.Name)
            .ToArrayAsync();
    }

    public async Task<CameraProfile> GetCameraProfileByIdAsync(Guid id)
    {
        _logger.LogTrace("Retrieving camera profile with ID {ProfileId}", id);
        var profile = await _db.Profiles.FindAsync(id);
        if (profile == null)
        {
            _logger.LogWarning("Camera profile with ID {ProfileId} not found", id);
            throw new KeyNotFoundException($"Camera profile with ID {id} not found.");
        }
        return profile;
    }

    public async Task<CameraProfile[]> GetCameraProfilesByBrandIdAsync(Guid brandId)
    {
        _logger.LogTrace("Retrieving camera profiles for brand with ID {BrandId}", brandId);
        return await _db.Profiles
            .AsNoTracking()
            .Where(p => p.Camera.BrandId == brandId)
            .OrderBy(p => p.Name)
            .ToArrayAsync();
    }

    public async Task<CameraProfile[]> GetCameraProfilesByIdsAsync(IEnumerable<Guid> ids)
    {
        _logger.LogTrace("Retrieving camera profiles with IDs {Ids}", string.Join(", ", ids));
        return await _db.Profiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToArrayAsync();
    }
}