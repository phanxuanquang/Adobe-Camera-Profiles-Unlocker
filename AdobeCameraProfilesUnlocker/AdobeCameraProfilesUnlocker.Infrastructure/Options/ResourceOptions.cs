namespace AdobeCameraProfilesUnlocker.Infrastructure.Options;

public sealed record ResourceOptions
{
    public required string AdobeStandardCameraProfilesDirectory { get; init; }
    public required string[] CameraProfileDirectories { get; init; }
    public double ThresholdForDataUpdateInDays { get; init; } = 0.1f;
}