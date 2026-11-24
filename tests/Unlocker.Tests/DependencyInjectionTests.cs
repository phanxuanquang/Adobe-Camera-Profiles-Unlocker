using Microsoft.Extensions.DependencyInjection;
using Unlocker.Infrastructure;
using Unlocker.Presentation;
using Unlocker.Presentation.Installed;
using Unlocker.Presentation.Ports;
using Unlocker.Presentation.Settings;
using Unlocker.Presentation.Shell;
using Unlocker.Presentation.Startup;
using Unlocker.Presentation.Unlock;
using Unlocker.Tests.Fakes;

namespace Unlocker.Tests;

[TestClass]
public sealed class DependencyInjectionTests
{
    /// <summary>Same options as App.xaml.cs: catches missing registrations and captive dependencies.</summary>
    [TestMethod]
    public async Task Graph_builds_and_resolves_every_view_model()
    {
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddInfrastructure()
            .AddPresentation()
            .AddSingleton<IDialogService, FakeDialogs>()
            .AddSingleton<ILauncher, FakeLauncher>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Assert.IsNotNull(provider.GetRequiredService<ShellViewModel>());
        Assert.IsNotNull(provider.GetRequiredService<UnlockViewModel>());
        Assert.IsNotNull(provider.GetRequiredService<InstalledViewModel>());
        Assert.IsNotNull(provider.GetRequiredService<SettingsViewModel>());
        Assert.IsNotNull(provider.GetRequiredService<StartupViewModel>());
        Assert.AreSame(provider.GetRequiredService<UnlockViewModel>(), provider.GetRequiredService<UnlockViewModel>());
        Assert.AreNotSame(provider.GetRequiredService<StartupViewModel>(), provider.GetRequiredService<StartupViewModel>());
    }
}
