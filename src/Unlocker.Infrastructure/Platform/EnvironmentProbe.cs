using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Enums;

namespace Unlocker.Infrastructure.Platform;

internal sealed class EnvironmentProbe(AppPaths paths, ILogger<EnvironmentProbe> logger) : IEnvironmentProbe
{
    public IReadOnlyList<EnvironmentIssue> Check()
    {
        if (!Directory.Exists(paths.CameraRawRoot))
        {
            logger.LogWarning("Camera Raw not found at {CameraRawRoot}", paths.CameraRawRoot);
            return [EnvironmentIssue.CameraRawMissing];
        }
        var missing = new[] { paths.DcpProfilesDir, paths.XmpProfilesDir }.Where(d => !Directory.Exists(d)).ToList();
        if (missing.Count == 0) return [];
        logger.LogWarning("Camera profile folders missing: {Folders}", string.Join(", ", missing));
        return [EnvironmentIssue.ProfilesMissing];
    }
}
