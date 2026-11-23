using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;

namespace Unlocker.Infrastructure.Installing;

internal sealed class CameraProfileInstaller(DcpProfileInstaller dcp, XmpProfileInstaller xmp, ILogger<CameraProfileInstaller> logger) : ICameraProfileInstaller
{
    public async Task<InstalledCameraProfile> InstallAsync(CameraProfile profile, CameraModel source, string targetModel, CancellationToken ct = default)
    {
        var output = profile.Format == CameraProfileFormat.DCP
            ? await dcp.InstallAsync(profile, source.Name, targetModel, ct)
            : await xmp.InstallAsync(profile, source.Name, targetModel, ct);
        logger.LogInformation("Installed profile {Profile} from {SourceModel} for {TargetModel} at {Output}",
            profile.Name, source.Name, targetModel, output);
        return new InstalledCameraProfile(profile.Name, source.Name, targetModel, [output]);
    }
}
