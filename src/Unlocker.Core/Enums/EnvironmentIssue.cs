namespace Unlocker.Core.Enums;

public enum EnvironmentIssue
{
    /// <summary>Blocks the app. The others only show a warning.</summary>
    CameraRawMissing,

    /// <summary>The Adobe DCP or XMP profile folder is missing. The log names which.</summary>
    ProfilesMissing,
}
