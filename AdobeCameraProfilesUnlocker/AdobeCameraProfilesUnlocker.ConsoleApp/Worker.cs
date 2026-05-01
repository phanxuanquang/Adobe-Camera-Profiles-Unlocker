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
                _logger.LogTrace("Camera brands found: {brands}", string.Join(", ", userCameraBrandSearchDict.Values));

                var userCameraBrandId = userCameraBrandSearchDict.FirstOrDefault().Key; // For demo purpose, just take the first matched brand
                _logger.LogInformation("User's camera brand: {brand}", userCameraBrandSearchDict.FirstOrDefault().Value);
                _logger.LogTrace("Getting camera models for brand '{brand}'", userCameraBrandSearchDict.FirstOrDefault().Value);

                var userCameraBrandSearchModels = await brandService.GetCameraModelsByBrandIdAsync(userCameraBrandId);
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

                var brandCameraProfileDict = await brandService.GetProfilesByBrandIdAsync(targetCameraBrandId);
                _logger.LogTrace("Found {count} camera profiles for brand '{brand}'", brandCameraProfileDict.Count, targetCameraBrandSearchDict.FirstOrDefault().Value);
                _logger.LogTrace("Camera profiles found: {profiles}", string.Join(", ", brandCameraProfileDict.Values));

                var userSelectedCameraProfileDict = brandCameraProfileDict
                    .Take(5)
                    .ToFrozenDictionary();
                var userSelectedCameraProfiles = await cameraProfileService.GetCameraProfilesByIdsAsync(userSelectedCameraProfileDict.Keys.ToArray());
                _logger.LogInformation("User selected camera profiles: {names}", string.Join(", ", userSelectedCameraProfileDict.Values));

                _logger.LogInformation("Adapting {count} camera profiles to user's camera model '{model}'", userSelectedCameraProfiles.Length, userCameraBrandSearchModels.FirstOrDefault().Value);
                await cameraProfileService.AdaptToCameraModelAsync(userSelectedCameraProfiles, userCameraModelId);

                _logger.LogInformation("Camera profile adaptation completed.");
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