using AdobeCameraProfilesUnlocker.Core.Models;

namespace AdobeCameraProfilesUnlocker.Core.Interfaces
{
    public interface IDcpTool
    {
        Task<bool> IsDcpToolInstalled();
        Task<CameraProfile> ConvertToDcpAsync(List<CameraProfile> profiles);
        Task<CameraProfile> ConvertToXmpAsync(List<CameraProfile> profiles);
    }
}
