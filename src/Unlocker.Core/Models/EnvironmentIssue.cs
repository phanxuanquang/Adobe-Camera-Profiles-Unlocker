namespace Unlocker.Core.Models;

public enum EnvironmentIssue
{
    /// <summary>Blocks the app. The others only show a warning.</summary>
    CameraRawMissing,

    DcpProfilesMissing,

    XmpProfilesMissing,
}
