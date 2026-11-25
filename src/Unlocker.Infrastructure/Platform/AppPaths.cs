namespace Unlocker.Infrastructure.Platform;

/// <summary>Every path the app reads or writes (docs/camera-profiles.md). Tests build one on temp folders.</summary>
internal sealed record AppPaths(
    string CameraRawRoot,
    string DcpProfilesDir,
    string AdobeStandardDir,
    string XmpProfilesDir,
    string DcpOutputDir,
    string XmpOutputDir,
    string AppDataDir)
{
    public string SettingsFile => Path.Combine(AppDataDir, "settings.json");
    public string ManifestFile => Path.Combine(AppDataDir, "installed.json");
    public string TrashDir => Path.Combine(AppDataDir, "trash");

    /// <summary>Adobe's base DCP for a camera: its sensor calibration. Nearly every supported camera has one.</summary>
    public string AdobeStandardFile(string model) => Path.Combine(AdobeStandardDir, $"{model} Adobe Standard.dcp");

    /// <summary>Camera-matching DCP by profile name ("Camera Standard", "Camera Standard v2"). Only some cameras have them.</summary>
    public string CameraProfileFile(string model, string profileName) => Path.Combine(DcpProfilesDir, model, $"{model} {profileName}.dcp");

    public static AppPaths Default { get; } = Create(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    /// <summary>Subfolder name for installed copies, under both Camera Raw profile roots.</summary>
    private const string OutputFolder = "Phan Xuan Quang";

    public static AppPaths Create(string programData, string localAppData)
    {
        var acr = Path.Combine(programData, "Adobe", "CameraRaw");
        return new AppPaths(
            CameraRawRoot: acr,
            DcpProfilesDir: Path.Combine(acr, "CameraProfiles", "Camera"),
            AdobeStandardDir: Path.Combine(acr, "CameraProfiles", "Adobe Standard"),
            XmpProfilesDir: Path.Combine(acr, "Settings", "Adobe", "Profiles", "Camera"),
            DcpOutputDir: Path.Combine(acr, "CameraProfiles", OutputFolder),
            XmpOutputDir: Path.Combine(acr, "Settings", OutputFolder),
            AppDataDir: Path.Combine(localAppData, "CameraProfilesUnlocker"));
    }
}
