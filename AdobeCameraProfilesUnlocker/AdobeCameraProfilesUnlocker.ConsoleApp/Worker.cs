using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using System.Collections.Frozen;

namespace AdobeCameraProfilesUnlocker.ConsoleApp;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMetadataService _metadataService;
    private readonly IBrandService _brandService;
    private readonly ICameraProfileService _cameraProfileService;
    public Worker(ILogger<Worker> logger,
        IMetadataService metadataService,
        IBrandService brandService,
        ICameraProfileService cameraProfileService)
    {
        _logger = logger;
        _metadataService = metadataService;
        _brandService = brandService;
        _cameraProfileService = cameraProfileService;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await _metadataService.EnsureDatasourceUpToDateAsync();

        _logger.LogInformation("Getting all camera brands");
        var brandDict = await _brandService.GetAllBrandsAsync();
        Console.Write("Please input a camera brand keyword to search for: ");
        var userCameraBrandKeyword = Console.ReadLine() ?? "Canon";
        var userCameraBrandSearchDict = brandDict
            .Where(x => x.Value.Contains(userCameraBrandKeyword, StringComparison.OrdinalIgnoreCase))
            .ToFrozenDictionary();
        _logger.LogTrace("Found {count} camera brands matching keyword '{keyword}'", userCameraBrandSearchDict.Count, userCameraBrandKeyword);
        _logger.LogTrace("Camera brands found: {brands}", string.Join(", ", userCameraBrandSearchDict.Values));

        var userCameraBrandId = userCameraBrandSearchDict.FirstOrDefault().Key; // For demo purpose, just take the first matched brand
        _logger.LogInformation("User's camera brand: {brand}", userCameraBrandSearchDict.FirstOrDefault().Value);
        _logger.LogTrace("Getting camera models for brand '{brand}'", userCameraBrandSearchDict.FirstOrDefault().Value);

        var userCameraBrandSearchModels = await _brandService.GetCameraModelsByBrandIdAsync(userCameraBrandId);
        _logger.LogTrace("Found {count} camera models for brand '{brand}'", userCameraBrandSearchModels.Count, userCameraBrandSearchDict.FirstOrDefault().Value);
        _logger.LogTrace("Camera models found: {models}", string.Join(", ", userCameraBrandSearchModels.Values));

        var userCameraModelId = userCameraBrandSearchModels.FirstOrDefault().Key; // For demo purpose, just take the first matched camera model
        _logger.LogInformation("User's camera model: {model}", userCameraBrandSearchModels.FirstOrDefault().Value);

        Console.Write("Please input a camera brand keyword to search for its profiles: ");
        var targetCameraBrandKeyword = Console.ReadLine() ?? "Nikon";

        var targetCameraBrandSearchDict = brandDict
            .Where(x => x.Value.Contains(targetCameraBrandKeyword, StringComparison.OrdinalIgnoreCase))
            .ToFrozenDictionary();
        _logger.LogTrace("Found {count} camera brands matching keyword '{keyword}'", targetCameraBrandSearchDict.Count, targetCameraBrandKeyword);
        _logger.LogTrace("Camera brands found: {brands}", string.Join(", ", targetCameraBrandSearchDict.Values));

        var targetCameraBrandId = targetCameraBrandSearchDict.FirstOrDefault().Key; // For demo purpose, just take the first matched brand
        _logger.LogInformation("Target camera brand for profile searching: {brand}", targetCameraBrandSearchDict.FirstOrDefault().Value);

        var brandCameraProfileDict = await _brandService.GetProfilesByBrandIdAsync(targetCameraBrandId);
        _logger.LogTrace("Found {count} camera profiles for brand '{brand}'", brandCameraProfileDict.Count, targetCameraBrandSearchDict.FirstOrDefault().Value);
        _logger.LogTrace("Camera profiles found: {profiles}", string.Join(", ", brandCameraProfileDict.Values));

        var userSelectedCameraProfileDict = brandCameraProfileDict
            .Take(5)
            .ToFrozenDictionary();
        var userSelectedCameraProfiles = await _cameraProfileService.GetCameraProfilesByIdsAsync(userSelectedCameraProfileDict.Keys.ToArray());
        _logger.LogInformation("User selected camera profiles: {names}", string.Join(", ", userSelectedCameraProfileDict.Values));

        _logger.LogInformation("Adapting {count} camera profiles to user's camera model '{model}'", userSelectedCameraProfiles.Length, userCameraBrandSearchModels.FirstOrDefault().Value);
        await _cameraProfileService.AdaptToCameraModelAsync(userSelectedCameraProfiles, userCameraModelId);

        _logger.LogInformation("Camera profile adaptation completed.");
        await Task.Delay(100000, stoppingToken);
    }
}