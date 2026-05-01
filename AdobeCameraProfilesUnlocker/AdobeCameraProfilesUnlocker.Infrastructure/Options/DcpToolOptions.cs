namespace AdobeCameraProfilesUnlocker.Infrastructure.Options;

public class DcpToolOptions
{
    public required string DcpToolExecutableDirectory { get; set; }
    public required string TargetDirectory { get; set; }
    public string DecompileOutputDirectory { get; set; } = Path.GetTempPath();
}