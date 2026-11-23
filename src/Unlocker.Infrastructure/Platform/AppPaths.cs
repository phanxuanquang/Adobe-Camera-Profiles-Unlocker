namespace Unlocker.Infrastructure.Platform;

/// <summary>Every path the app reads or writes (docs/adobe-paths.md). Tests build one on temp folders.</summary>
internal sealed record AppPaths(
    string CameraRawRoot,
    string DcpProfilesDir,
    string XmpProfilesDir,
    string DcpOutputDir,
    string XmpOutputDir,
    string LegacyDcpDir,
    string AppDataDir)
{
    public string SettingsFile => Path.Combine(AppDataDir, "settings.json");
    public string ManifestFile => Path.Combine(AppDataDir, "installed.json");
    public string TrashDir => Path.Combine(AppDataDir, "trash");
    public static string DcpToolExe => Path.Combine(AppContext.BaseDirectory, "Assets", "dcpTool.exe");

    public static AppPaths Default { get; } = Create(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    public static AppPaths Create(string programData, string localAppData)
    {
        var acr = Path.Combine(programData, "Adobe", "CameraRaw");
        return new AppPaths(
            CameraRawRoot: acr,
            DcpProfilesDir: Path.Combine(acr, "CameraProfiles", "Camera"),
            XmpProfilesDir: Path.Combine(acr, "Settings", "Adobe", "Profiles", "Camera"),
            DcpOutputDir: Path.Combine(acr, "CameraProfiles", "Unlocker"),
            XmpOutputDir: Path.Combine(acr, "Settings", "Unlocker"),
            LegacyDcpDir: Path.Combine(acr, "CameraProfiles"),
            AppDataDir: Path.Combine(localAppData, "CameraProfilesUnlocker"));
    }
}
