namespace Unlocker.Core.Enums;

public enum EnvironmentIssue
{
    /// <summary>Blocks the app. The others only show a warning.</summary>
    CameraRawMissing,

    DcpProfilesMissing,

    XmpProfilesMissing,
}
