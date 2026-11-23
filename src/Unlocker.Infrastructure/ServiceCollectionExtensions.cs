using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Catalog;
using Unlocker.Infrastructure.Installing;
using Unlocker.Infrastructure.Platform;
using Unlocker.Infrastructure.Storage;
using Unlocker.Infrastructure.Updates;

namespace Unlocker.Infrastructure;

/// <summary>Lifetimes: docs/di-lifetimes.md.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(AppPaths.Default);
        services.AddSingleton<ICameraCatalog, FileCameraCatalog>();
        services.AddSingleton<CameraProfileRewriter>();
        services.AddSingleton<DcpProfileInstaller>();
        services.AddSingleton<XmpProfileInstaller>();
        services.AddSingleton<ICameraProfileInstaller, CameraProfileInstaller>();
        services.AddSingleton<IInstalledStore, JsonInstalledStore>();
        services.AddSingleton<ISettings, JsonSettings>();
        services.AddTransient<IEnvironmentProbe, EnvironmentProbe>();
        var http = new HttpClient { BaseAddress = new Uri("https://api.github.com/"), Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("CameraProfilesUnlocker");
        services.AddSingleton<IUpdateService>(sp => new GitHubUpdateService(http, sp.GetRequiredService<ILogger<GitHubUpdateService>>()));
        return services;
    }
}
