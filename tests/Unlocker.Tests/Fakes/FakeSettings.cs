using Unlocker.Core.Abstractions;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeSettings : ISettings
{
    public string? TargetCamera { get; set; }

    public Exception? FailSaveWith { get; set; }

    public Task SaveAsync(CancellationToken ct = default) => FailSaveWith is { } ex ? Task.FromException(ex) : Task.CompletedTask;
}
