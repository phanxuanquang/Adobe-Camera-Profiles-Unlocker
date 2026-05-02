using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Infrastructure.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AdobeCameraProfilesUnlocker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIOService(this IServiceCollection services)
    {
        services
            .AddHttpClient(nameof(IOService))
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1.5),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(45),
                ConnectTimeout = TimeSpan.FromSeconds(30),
                AutomaticDecompression = System.Net.DecompressionMethods.All,
                MaxConnectionsPerServer = 4,
                EnableMultipleHttp2Connections = true,
                InitialHttp2StreamWindowSize = 8 * 1024 * 1024,
            });

        services.AddSingleton<IIOService, IOService>();
        return services;
    }
}