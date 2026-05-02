using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models;
using AdobeCameraProfilesUnlocker.Domain.Models.Enums;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using AdobeCameraProfilesUnlocker.Infrastructure.Helpers;
using AdobeCameraProfilesUnlocker.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class CameraProfileService(
    AppDbContext db,
    IIOService ioService,
    IDcpToolService dcpToolService,
    IOptionsSnapshot<DcpToolOptions> dcpToolOptions,
    ILoggerFactory? loggerFactory = null) : ICameraProfileService
{
    private readonly AppDbContext _db = db;
    private readonly IIOService _ioService = ioService;
    private readonly IDcpToolService _dcpToolService = dcpToolService;
    private readonly ILogger<CameraProfileService> _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CameraProfileService>();
    private readonly DcpToolOptions _dcpToolOptions = dcpToolOptions.Value;

    public async Task AdaptToCameraModelAsync(CameraProfile[] profiles, Guid cameraModelId)
    {
        var profileIds = profiles
            .Where(p => p.CameraId != cameraModelId && p.FileType == CameraProfileType.DCP)
            .Select(p => p.Id)
            .Where(id => !File.Exists(Path.Combine(_dcpToolOptions.DecompileOutputDirectory, $"{id}.xml"))
                && !File.Exists(Path.Combine(_dcpToolOptions.TargetDirectory, $"{id}.dcp")))
            .ToArray();

        if (profileIds.Length == 0)
        {
            _logger.LogWarning("All profiles for camera model with ID {CameraModelId} are already decompiled or adapted", cameraModelId);
            return;
        }

        await _dcpToolService.DecompileDcpIntoXmlAsync(profileIds);

        var xmlFilePaths = profileIds
            .Select(id => Path.Combine(_dcpToolOptions.DecompileOutputDirectory, $"{id}.xml"))
            .ToArray();

        var cameraModel = await _db.Cameras
            .AsNoTracking()
            .Where(c => c.Id == cameraModelId)
            .Select(c => c.CodeName)
            .FirstAsync();

        foreach (var filePath in xmlFilePaths)
        {
            _logger.LogTrace("Updating XML attributes for profile with file path {FilePath}", filePath);
            XmlHelper.UpdateAttributes(filePath, new Dictionary<string, string>
            {
                { "Copyright", "© 2026 Phan Xuan Quang / Github: @phanxuanquang" },
                { "ProfileCalibrationSignature", "Phan Xuan Quang" },
                { "ProfileName", cameraModel }
            });
        }

        await _dcpToolService.CompileXmlIntoDcpAsync(profileIds);

        foreach (var filePath in xmlFilePaths)
        {
            await _ioService.DeleteFileAsync(filePath);
        }
    }

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

    public async Task<CameraProfile[]> GetCameraProfilesByIdsAsync(Guid[] ids)
    {
        _logger.LogTrace("Retrieving camera profiles with IDs {Ids}", string.Join(", ", ids));
        return await _db.Profiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToArrayAsync();
    }
}