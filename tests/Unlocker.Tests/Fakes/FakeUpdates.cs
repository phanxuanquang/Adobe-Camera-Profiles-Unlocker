using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeUpdates : IUpdateService
{
    public ReleaseInfo? Next { get; set; }
    public Exception? FailWith { get; set; }

    public Task<ReleaseInfo?> GetNewerAsync(Version current, CancellationToken ct = default)
        => FailWith is null ? Task.FromResult(Next) : Task.FromException<ReleaseInfo?>(FailWith);
}
