using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class CameraProfileService : ICameraProfileService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CameraProfileService> _logger;

    public CameraProfileService(AppDbContext db, ILoggerFactory? loggerFactory = null)
    {
        _db = db;
        _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CameraProfileService>();
    }

    public async Task AdaptToCameraModelAsync(CameraProfile[] profiles, Guid cameraModelId)
    {
        _logger.LogTrace("Adapting {Count} camera profiles to camera model with ID {CameraModelId}", profiles.Length, cameraModelId);
        // TODO: Implement the logic to adapt camera profiles to the specified camera model.
        foreach (var profile in profiles)
        {
            _logger.LogInformation("Adapting camera profile with ID {ProfileId} to camera model with ID {CameraModelId}", profile.Id, cameraModelId);
            // Placeholder for adaptation logic
            await Task.Delay(100); // Simulate some asynchronous work
        }
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
            .Where(p => p.BrandId == brandId)
            .ToArrayAsync();
    }

    public async Task<CameraProfile[]> GetCameraProfilesByIdsAsync(Guid[] ids)
    {
        _logger.LogTrace("Retrieving camera profiles with IDs {Ids}", string.Join(", ", ids));
        return await _db.Profiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToArrayAsync();
    }
}