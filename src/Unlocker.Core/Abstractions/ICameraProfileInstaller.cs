using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

public interface ICameraProfileInstaller
{
    /// <summary>
    /// Writes a rewritten copy of <paramref name="profile"/> restricted to <paramref name="targetModel"/>.
    /// Never modifies the source file. Overwrites an earlier copy of the same profile for the same target.
    /// Does not touch <see cref="IInstalledStore"/>: the caller records the result.
    /// </summary>
    Task<InstalledCameraProfile> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct = default);
}
