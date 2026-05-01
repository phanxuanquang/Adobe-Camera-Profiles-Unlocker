using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases.Models;
using AdobeCameraProfilesUnlocker.Infrastructure.Helpers;
using AdobeCameraProfilesUnlocker.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class LocalResourceService : IResourceService
{
    private readonly ILogger<LocalResourceService> _logger;
    private readonly AppDbContext _db;
    private readonly ResourceOptions _metadataOptions;
    public LocalResourceService(
        AppDbContext db,
        IOptionsSnapshot<ResourceOptions> metadataOptions,
        ILogger<LocalResourceService>? logger = null)
    {
        _db = db;
        _metadataOptions = metadataOptions.Value;
        _logger = logger ?? NullLogger<LocalResourceService>.Instance;

        _logger.LogInformation("{Service} initialized with AdobeStandardCameraProfilesDirectory: {AdobeStandardCameraProfilesDirectory}, ThresholdForDataUpdateInDays: {ThresholdForDataUpdateInDays}",
            nameof(LocalResourceService), _metadataOptions.AdobeStandardCameraProfilesDirectory, _metadataOptions.ThresholdForDataUpdateInDays);
    }

    public async Task EnsureDatasourceUpToDateAsync()
    {
        _logger.LogTrace("Checking if datasource is up to date.");
        var metadata = await _db.Metas.FirstOrDefaultAsync();

        if (metadata == null)
        {
            _logger.LogWarning("No datasource found.");
            await ForceUpdateDatasourceAsync();

            await _db.Metas.AddAsync(new Meta());
            await _db.SaveChangesAsync();

            return;
        }

        if (metadata.LastUpdatedTime < DateTime.UtcNow.AddDays(-_metadataOptions.ThresholdForDataUpdateInDays))
        {
            _logger.LogWarning("Datasource is outdated due to exceeding the threshold of {ThresholdForDataUpdateInDays} days.", _metadataOptions.ThresholdForDataUpdateInDays);
            await ForceUpdateDatasourceAsync();
            metadata.LastUpdatedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        _logger.LogInformation("Datasource is up to date, last updated at {LastUpdatedTime}.", metadata.LastUpdatedTime);
    }

    public async Task ForceUpdateDatasourceAsync()
    {
        _logger.LogTrace("Force updating datasource...");
        var cameraWithBrandDict = IOHelper.EnumerateFilesSafe(_metadataOptions.AdobeStandardCameraProfilesDirectory)
            .AsParallel()
            .Select(filePath =>
            {
                var name = Path.GetFileNameWithoutExtension(filePath);

                name = name
                    .Replace(" Adobe Standard", string.Empty)
                    .Replace(" Adobe_Standard", string.Empty)
                    .Replace(" Camera Default", string.Empty)
                    .Trim();

                var spaceIndex = name.IndexOf(' ');
                var brand = spaceIndex < 0 ? name : name[..spaceIndex];

                return new
                {
                    CameraModel = name,
                    Brand = brand
                };
            })
            .DistinctBy(x => x.CameraModel)
            .ToFrozenDictionary(x => x.CameraModel, x => x.Brand);

        if (cameraWithBrandDict.Count == 0)
        {
            _logger.LogWarning("No camera profiles found in the directory: {AdobeStandardCameraProfilesDirectory}", _metadataOptions.AdobeStandardCameraProfilesDirectory);
            return;
        }

        var brandNames = cameraWithBrandDict.Values
            .AsParallel()
            .Select(name => name.ToLower())
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        var existingBrands = await _db.Brands
            .AsNoTracking()
            .Where(b => brandNames.Contains(b.Name.ToLower()))
            .Select(b => b.Name.ToLower())
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        if (existingBrands.Count != brandNames.Count)
        {
            var newBrands = cameraWithBrandDict.Values
                .Where(name => !existingBrands.Contains(name.ToLower()))
                .AsParallel()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => new CameraBrand
                {
                    Name = name
                })
                .ToArray();
            _logger.LogTrace("Found {NewBrandCount} new camera brands.", newBrands.Length);

            await _db.Brands.AddRangeAsync(newBrands);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added {NewBrandCount} new camera brands into the database.", newBrands.Length);
        }

        var cameraNames = cameraWithBrandDict.Keys
            .AsParallel()
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        var existingCameras = await _db.Cameras
            .AsNoTracking()
            .Where(c => cameraNames.Contains(c.CodeName))
            .Select(c => c.CodeName)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        if (existingCameras.Count != cameraNames.Count)
        {
            var cameraBrandMap = await _db.Brands
                .AsNoTracking()
                .Where(b => brandNames.Contains(b.Name.ToLower()))
                .ToDictionaryAsync(b => b.Name.ToLower(), b => b.Id, StringComparer.OrdinalIgnoreCase);

            var newCameras = cameraWithBrandDict
                .Where(kv => !existingCameras.Contains(kv.Key))
                .AsParallel()
                .DistinctBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => new CameraModel
                {
                    CodeName = kv.Key,
                    BrandId = cameraBrandMap[kv.Value.ToLower()]
                })
                .ToArray();
            _logger.LogTrace("Found {NewCameraCount} new camera models.", newCameras.Length);

            await _db.Cameras.AddRangeAsync(newCameras);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added {NewCameraCount} new camera models into the database.", newCameras.Length);
        }
    }
}