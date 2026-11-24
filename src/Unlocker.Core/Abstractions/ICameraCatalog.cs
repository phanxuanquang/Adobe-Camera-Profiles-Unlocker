using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

public interface ICameraCatalog
{
    /// <summary>Brands, models and camera profiles, each sorted by name. Scans once, then cached.</summary>
    Task<IReadOnlyList<Brand>> LoadAsync(CancellationToken ct = default);
}
