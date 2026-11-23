using Unlocker.Core.Abstractions;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeSettings : ISettings
{
    public string? TargetCamera { get; set; }

    public Task SaveAsync(CancellationToken ct = default) => Task.CompletedTask;
}
