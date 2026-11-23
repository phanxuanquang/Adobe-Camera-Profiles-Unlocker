using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeUpdates : IUpdateService
{
    public ReleaseInfo? Next { get; set; }

    public Task<ReleaseInfo?> GetNewerAsync(Version current, CancellationToken ct = default) => Task.FromResult(Next);
}
