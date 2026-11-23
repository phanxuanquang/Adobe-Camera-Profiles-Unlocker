using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Unlocker.Infrastructure;
using Unlocker.Presentation;
using Unlocker.Presentation.Ports;
using Unlocker.WinUI.Services;

namespace Unlocker.WinUI;

public partial class App : Application
{
    private static ServiceProvider? _services;

    public App() => InitializeComponent();

    /// <summary>Only for the composition root and page constructors: Frame creates pages, so they cannot take constructor injection.</summary>
    public static IServiceProvider Services => _services ?? throw new InvalidOperationException("App not launched.");

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _services = new ServiceCollection()
            .AddLogging(logging => logging
                .SetMinimumLevel(LogLevel.Information)
                .AddDebug()
                .AddEventLog(o => o.Filter = (_, level) => level >= LogLevel.Information))
            .AddInfrastructure()
            .AddPresentation()
            .AddSingleton<MainWindow>().AddSingleton<IDialogService, DialogService>().AddSingleton<ILauncher, Launcher>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        var logger = _services.GetRequiredService<ILogger<App>>();
        UnhandledException += (_, e) => logger.LogCritical(e.Exception, "Unhandled exception");

        var window = _services.GetRequiredService<MainWindow>();
        // DisposeAsync, not Dispose: the installed store is IAsyncDisposable and empties the trash.
        window.Closed += async (_, _) => await _services.DisposeAsync();
        window.Activate();
    }
}
