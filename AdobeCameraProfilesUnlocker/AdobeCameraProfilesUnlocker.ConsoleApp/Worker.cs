using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;

namespace AdobeCameraProfilesUnlocker.ConsoleApp;

public class Worker(
        ILogger<Worker> logger,
        IServiceScopeFactory scopeFactory,
        IDbContextFactory<AppDbContext> dbContextFactory)
    : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                await using var db = await _dbContextFactory.CreateDbContextAsync(stoppingToken).ConfigureAwait(false);
                await db.Database.EnsureCreatedAsync(stoppingToken).ConfigureAwait(false);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var metadataService = scope.ServiceProvider.GetRequiredService<IResourceService>();
                var brandService = scope.ServiceProvider.GetRequiredService<IBrandService>();
                var cameraProfileService = scope.ServiceProvider.GetRequiredService<ICameraProfileService>();

                await metadataService.EnsureDatasourceUpToDateAsync();

                _logger.LogInformation("Getting all camera brands");
                var brandDict = await brandService.GetAllBrandsAsync();
                Console.Write("Please input a camera brand keyword to search for: ");
                var userCameraBrandKeyword = Console.ReadLine() ?? "Canon";
                var userCameraBrandSearchDict = brandDict
                    .Where(x => x.Value.Contains(userCameraBrandKeyword, StringComparison.OrdinalIgnoreCase))
                    .ToFrozenDictionary();
                _logger.LogTrace("Found {count} camera brands matching keyword '{keyword}'", userCameraBrandSearchDict.Count, userCameraBrandKeyword);
                _logger.LogTrace("Camera brands found:\n - {brands}", string.Join("\n - ", userCameraBrandSearchDict.Values));

                var userCameraBrandId = userCameraBrandSearchDict.FirstOrDefault().Key; // For demo purpose, just take the first matched brand
                _logger.LogInformation("User's camera brand: {brand}", userCameraBrandSearchDict.FirstOrDefault().Value);
                _logger.LogTrace("Getting camera models for brand '{brand}'", userCameraBrandSearchDict.FirstOrDefault().Value);

                var userCameraBrandSearchModels = await brandService.GetCameraModelsByBrandIdAsync(userCameraBrandId);
                _logger.LogTrace("Found {count} camera models for brand '{brand}'", userCameraBrandSearchModels.Count, userCameraBrandSearchDict.FirstOrDefault().Value);
                _logger.LogTrace("Camera models found:\n - {models}", string.Join("\n - ", userCameraBrandSearchModels.Values));

                var userCameraModelId = userCameraBrandSearchModels.FirstOrDefault().Key; // For demo purpose, just take the first matched camera model
                _logger.LogInformation("User's camera model: {model}", userCameraBrandSearchModels.FirstOrDefault().Value);

                Console.Write("Please input a camera brand keyword to search for its profiles: ");
                var targetCameraBrandKeyword = Console.ReadLine() ?? "Nikon";

                var targetCameraBrandSearchDict = brandDict
                    .Where(x => x.Value.Contains(targetCameraBrandKeyword, StringComparison.OrdinalIgnoreCase))
                    .ToFrozenDictionary();
                _logger.LogTrace("Found {count} camera brands matching keyword '{keyword}'", targetCameraBrandSearchDict.Count, targetCameraBrandKeyword);
                _logger.LogTrace("Camera brands found:\n - {brands}", string.Join("\n - ", targetCameraBrandSearchDict.Values));

                var targetBrandId = targetCameraBrandSearchDict.FirstOrDefault().Key; // For demo purpose, just take the first matched brand
                _logger.LogInformation("Target camera brand for camera model searching: {brand}", targetCameraBrandSearchDict.FirstOrDefault().Value);

                var targetBrandCameraSearchDict = await brandService.GetCameraModelsByBrandIdAsync(targetBrandId);
                _logger.LogInformation("Found {count} camera models for brand '{brand}':\n - {models}", targetBrandCameraSearchDict.Count, targetCameraBrandSearchDict.FirstOrDefault().Value, string.Join("\n - ", targetBrandCameraSearchDict.Values));

                var targetCameraModelId = targetBrandCameraSearchDict.FirstOrDefault().Key; // For demo purpose, just take the first matched camera model
                _logger.LogInformation("Target camera model for profile searching: {model}", targetBrandCameraSearchDict.FirstOrDefault().Value);

                var targetCameraProfiles = await cameraProfileService.GetCameraProfileByCameraIdAsync(targetCameraModelId);
                _logger.LogInformation("Found {count} camera profiles for camera model '{model}':\n - {profiles}", targetCameraProfiles.Length, targetBrandCameraSearchDict.FirstOrDefault().Value, string.Join("\n - ", targetCameraProfiles.Select(p => p.Name)));

                var userSelectedCameraProfiles = targetCameraProfiles.Take(3).ToArray(); // For demo purpose, just take the first 3 profiles
                _logger.LogInformation("User selected {count} camera profiles for adaptation: \n - {profiles}", userSelectedCameraProfiles.Length, string.Join("\n - ", userSelectedCameraProfiles.Select(p => p.Name)));

                _logger.LogInformation("Adapting selected camera profiles to user's camera model...");
                await cameraProfileService.AdaptToCameraModelAsync(userSelectedCameraProfiles, userCameraModelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the worker.");
            }
            finally
            {
                _logger.LogInformation("Worker completed execution at: {time}", DateTimeOffset.Now);
                await Task.Delay(100000, stoppingToken);
            }

        }
    }
}