using Microsoft.Extensions.DependencyInjection;
using Unlocker.Presentation.Installed;
using Unlocker.Presentation.Settings;
using Unlocker.Presentation.Shell;
using Unlocker.Presentation.Startup;
using Unlocker.Presentation.Unlock;

namespace Unlocker.Presentation;

/// <summary>The UI project registers the Ports. Lifetimes: docs/architecture.md.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<UnlockViewModel>();
        services.AddSingleton<InstalledViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddTransient<StartupViewModel>();
        return services;
    }
}
