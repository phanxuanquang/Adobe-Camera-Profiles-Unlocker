using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeInstaller : ICameraProfileInstaller
{
    public Func<CameraProfile, Exception?> FailWhen { get; set; } = _ => null;

    public Task<InstalledCameraProfile> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (FailWhen(profile) is { } ex) throw ex;
        return Task.FromResult(new InstalledCameraProfile(profile.Name, sourceModel, targetModel, [$"C:\\fake\\{profile.Name}"]));
    }
}
