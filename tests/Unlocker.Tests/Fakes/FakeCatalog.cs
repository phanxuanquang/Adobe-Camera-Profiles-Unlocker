using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeCatalog(params Brand[] brands) : ICameraCatalog
{
    public Task<IReadOnlyList<Brand>> LoadAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Brand>>(brands);
}
