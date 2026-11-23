using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

public interface IUpdateService
{
    /// <summary>Newer release, or null when up to date or on any error. Throws only on cancel.</summary>
    Task<ReleaseInfo?> GetNewerAsync(Version current, CancellationToken ct = default);
}
