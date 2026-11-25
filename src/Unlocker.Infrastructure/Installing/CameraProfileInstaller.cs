using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;

namespace Unlocker.Infrastructure.Installing;

internal sealed class CameraProfileInstaller(DcpProfileInstaller dcp, XmpProfileInstaller xmp, ILogger<CameraProfileInstaller> logger) : ICameraProfileInstaller
{
    public async Task<InstalledCameraProfile> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct = default)
    {
        IReadOnlyList<string> files = profile.Format == CameraProfileFormat.DCP
            ? [await dcp.InstallAsync(profile, sourceModel, targetModel, ct)]
            : await xmp.InstallAsync(profile, sourceModel, targetModel, ct);
        logger.LogInformation("Installed profile {Profile} from {SourceModel} for {TargetModel} at {Output}",
            profile.Name, sourceModel, targetModel, string.Join(", ", files));
        return new InstalledCameraProfile(profile.Name, sourceModel, targetModel, files);
    }
}
