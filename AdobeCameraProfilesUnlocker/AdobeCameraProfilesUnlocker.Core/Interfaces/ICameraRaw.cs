namespace AdobeCameraProfilesUnlocker.Core.Interfaces
{
    public interface ICameraRaw
    {
        Task<bool> IsCameraInstalledAsync();
        Task EnsureOutputDirectoryCreatedAsync();
    }
}
