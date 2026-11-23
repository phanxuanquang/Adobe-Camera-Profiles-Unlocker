using Microsoft.Extensions.Logging;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;

namespace Unlocker.Infrastructure.Platform;

internal sealed class EnvironmentProbe(AppPaths paths, ILogger<EnvironmentProbe> logger) : IEnvironmentProbe
{
    public IReadOnlyList<EnvironmentIssue> Check()
    {
        var issues = new List<EnvironmentIssue>();
        if (!Directory.Exists(paths.CameraRawRoot)) issues.Add(EnvironmentIssue.CameraRawMissing);
        else
        {
            if (!Directory.Exists(paths.DcpProfilesDir)) issues.Add(EnvironmentIssue.DcpProfilesMissing);
            if (!Directory.Exists(paths.XmpProfilesDir)) issues.Add(EnvironmentIssue.XmpProfilesMissing);
        }
        if (issues.Count > 0)
            logger.LogWarning("Environment issues under {CameraRawRoot}: {Issues}", paths.CameraRawRoot, string.Join(", ", issues));
        return issues;
    }
}
