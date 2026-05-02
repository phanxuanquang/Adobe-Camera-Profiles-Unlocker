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

public class LocalResourceService(
    AppDbContext db,
    IIOService ioService,
    IOptionsSnapshot<MetadataOptions> metadataOptions,
    ILogger<LocalResourceService>? logger = null) : IResourceService
{
    private readonly ILogger<LocalResourceService> _logger = logger ?? NullLogger<LocalResourceService>.Instance;
    private readonly MetadataOptions _options = metadataOptions.Value;
    private readonly AppDbContext _db = db;
    private readonly IIOService _ioService = ioService;

    public async Task EnsureDatasourceUpToDateAsync()
    {
        await _db.Database.EnsureCreatedAsync();
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

        if (metadata.LastUpdatedTime < DateTime.UtcNow.AddDays(-_options.ThresholdForDataUpdateInDays))
        {
            _logger.LogWarning("Datasource is outdated due to exceeding the threshold of {ThresholdForDataUpdateInDays} days.", _options.ThresholdForDataUpdateInDays);
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

        var cameraWithBrandDict = _ioService.EnumerateFiles(_options.AdobeStandardCameraProfilesDirectory)
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
                return (CameraModel: name, Brand: brand);
            })
            .DistinctBy(x => x.CameraModel, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(x => x.CameraModel, x => x.Brand, StringComparer.OrdinalIgnoreCase);

        if (cameraWithBrandDict.Count == 0)
        {
            _logger.LogWarning("No camera profiles found in the directory: {AdobeStandardCameraProfilesDirectory}", _options.AdobeStandardCameraProfilesDirectory);
            return;
        }

        #region Sync camera brands
        var distinctBrandNames = cameraWithBrandDict.Values
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        var existingBrands = await _db.Brands
            .AsNoTracking()
            .Where(b => distinctBrandNames.Contains(b.Name))
            .Select(b => b.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        if (existingBrands.Count != distinctBrandNames.Count)
        {
            var newBrands = distinctBrandNames
                .Where(name => !existingBrands.Contains(name))
                .Select(name => new CameraBrand { Name = name })
                .ToArray();

            _logger.LogTrace("Found {NewBrandCount} new camera brands.", newBrands.Length);
            await _db.Brands.AddRangeAsync(newBrands);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added {NewBrandCount} new camera brands into the database.", newBrands.Length);
        }
        #endregion

        #region Sync camera models
        var cameraNames = cameraWithBrandDict.Keys
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
                .Where(b => distinctBrandNames.Contains(b.Name))
                .ToDictionaryAsync(b => b.Name, b => b.Id, StringComparer.OrdinalIgnoreCase);

            var newCameras = cameraWithBrandDict
                .Where(kv => !existingCameras.Contains(kv.Key))
                .Select(kv => new CameraModel
                {
                    CodeName = kv.Key,
                    BrandId = cameraBrandMap[kv.Value]
                })
                .ToArray();

            _logger.LogTrace("Found {NewCameraCount} new camera models.", newCameras.Length);
            await _db.Cameras.AddRangeAsync(newCameras);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added {NewCameraCount} new camera models into the database.", newCameras.Length);
        }
        #endregion

        #region Sync camera profiles
        var cameraWithIdDict = await _db.Cameras
            .AsNoTracking()
            .Where(c => cameraNames.Contains(c.CodeName))
            .ToDictionaryAsync(c => c.CodeName, c => c.Id, StringComparer.OrdinalIgnoreCase);

        var cameraProfileTypeByExtension = Enum.GetValues<CameraProfileType>()
            .ToFrozenDictionary(t => $".{t.ToString().ToLower()}", t => t, StringComparer.OrdinalIgnoreCase);

        var profileFileLookup = _options.CameraProfileDirectories
            .AsParallel()
            .SelectMany(dir => _ioService.EnumerateFiles(dir))
            .Select(fp =>
            {
                var ext = Path.GetExtension(fp);
                if (!cameraProfileTypeByExtension.TryGetValue(ext, out var fileType))
                    return default;

                var stem = Path.GetFileNameWithoutExtension(fp);
                var cameraIdx = stem.IndexOf(" Camera ", StringComparison.OrdinalIgnoreCase);
                var modelName = cameraIdx >= 0 ? stem[..cameraIdx] : stem;
                var profileName = cameraIdx >= 0 ? stem[(cameraIdx + " Camera ".Length)..] : "Default";

                return (FilePath: fp, FileType: fileType, ModelName: modelName, ProfileName: profileName, IsValid: true);
            })
            .Where(x => x.IsValid)
            .ToLookup(x => x.ModelName, StringComparer.OrdinalIgnoreCase);

        var seenProfiles = new HashSet<(string Name, Guid CameraId)>(ProfileKeyComparer.Instance);
        var cameraProfiles = new List<CameraProfile>();

        foreach (var (cameraModel, _) in cameraWithBrandDict)
        {
            if (!cameraWithIdDict.TryGetValue(cameraModel, out var defaultCameraId))
                continue;

            foreach (var (FilePath, FileType, ModelName, ProfileName, IsValid) in profileFileLookup[cameraModel])
            {
                var cameraId = cameraWithIdDict.TryGetValue(ModelName, out var id) ? id : defaultCameraId;

                if (!seenProfiles.Add((ProfileName, cameraId)))
                    continue;

                cameraProfiles.Add(new CameraProfile
                {
                    FilePath = FilePath,
                    CameraId = cameraId,
                    Name = ProfileName,
                    FileType = FileType,
                });
            }
        }

        var relevantCameraIds = cameraProfiles
            .Select(p => p.CameraId)
            .ToFrozenSet();

        var existingProfileSet = (await _db.Profiles
            .AsNoTracking()
            .Where(p => relevantCameraIds.Contains(p.CameraId))
            .Select(p => new { p.Name, p.CameraId })
            .ToListAsync())
            .Select(p => (p.Name, p.CameraId))
            .ToFrozenSet(ProfileKeyComparer.Instance);

        var newProfiles = cameraProfiles
            .Where(p => !existingProfileSet.Contains((p.Name, p.CameraId)))
            .ToArray();

        _logger.LogTrace("Found {NewProfileCount} new camera profiles.", newProfiles.Length);

        if (newProfiles.Length > 0)
        {
            await _db.Profiles.AddRangeAsync(newProfiles);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added {NewProfileCount} new camera profiles into the database.", newProfiles.Length);
        }
        #endregion
    }
}