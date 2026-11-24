namespace Unlocker.Core.Abstractions;

public interface ISettings
{
    /// <summary>The user's camera model. Null on first run.</summary>
    string? TargetCamera { get; set; }

    Task SaveAsync(CancellationToken ct = default);
}
