using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

public interface IUpdateService
{
    /// <summary>Newer release, or null when up to date. Throws when the check fails (network, bad response).</summary>
    Task<ReleaseInfo?> GetNewerAsync(Version current, CancellationToken ct = default);
}
