using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models;
using AdobeCameraProfilesUnlocker.Domain.Models.Enums;
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

        #region Sync camera brands
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
        #endregion

        #region Sync camera models
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
        #endregion

        #region Sync camera profiles

        var cameraBrandDict = await _db.Brands
            .AsNoTracking()
            .ToDictionaryAsync(b => b.Name.ToLower(), b => b.Id, StringComparer.OrdinalIgnoreCase);

        var cameraProfileTypeFileExtensionDict = ((CameraProfileType[])Enum.GetValues(typeof(CameraProfileType)))
            .ToFrozenDictionary(t => $".{t.ToString().ToLower()}", t => t, StringComparer.OrdinalIgnoreCase);

        var profileFiles = _metadataOptions.CameraProfileDirectories
            .AsParallel()
            .SelectMany(dir => IOHelper.EnumerateFilesSafe(dir))
            .Where(filePath => cameraProfileTypeFileExtensionDict.ContainsKey(Path.GetExtension(filePath)))
            .Select(filePath =>
            {
                var cameraModel = Path.GetFileName(Path.GetDirectoryName(filePath))!;
                var profileName = Path.GetFileNameWithoutExtension(filePath)
                    .Replace(cameraModel, string.Empty)
                    .Replace(" Camera ", string.Empty)
                    .Trim();

                var fileExtension = Path.GetExtension(filePath);
                var brandId = cameraBrandDict
                    .Where(dict => filePath.Contains(dict.Key, StringComparison.OrdinalIgnoreCase))
                    .Select(dict => dict.Value)
                    .FirstOrDefault();

                if (brandId == default)
                {
                    _logger.LogWarning("No matching brand found for camera profile file: {FilePath}", filePath);
                }

                return new CameraProfile
                {
                    Name = profileName,
                    FilePath = filePath,
                    FileType = cameraProfileTypeFileExtensionDict[fileExtension],
                    BrandId = brandId == default
                        ? null
                        : brandId
                };
            })
            .DistinctBy(p => (p.Name, p.BrandId))
            .ToArray();

        var profileNames = profileFiles
            .AsParallel()
            .Select(p => p.Name.ToLower())
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        var profileBrandIds = profileFiles
            .AsParallel()
            .Select(p => p.BrandId)
            .ToFrozenSet();

        var existingProfiles = await _db.Profiles
            .AsNoTracking()
            .Where(p => profileNames.Contains(p.Name.ToLower()) && profileBrandIds.Contains(p.BrandId))
            .Select(p => new { p.Name, p.BrandId })
            .ToArrayAsync();

        if (existingProfiles.Length != profileFiles.Length)
        {
            var newProfiles = profileFiles
                .Where(p => !existingProfiles.Any(ep => ep.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase) && ep.BrandId == p.BrandId))
                .AsParallel()
                .ToArray();

            _logger.LogTrace("Found {NewProfileCount} new camera profiles.", newProfiles.Length);
            await _db.Profiles.AddRangeAsync(newProfiles);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added {NewProfileCount} new camera profiles into the database.", newProfiles.Length);
        }

        #endregion
    }
}