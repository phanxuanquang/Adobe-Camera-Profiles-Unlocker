
using AdobeCameraProfilesUnlocker.Core.Models;

namespace AdobeCameraProfilesUnlocker.Interfaces
{
    public interface ICameraMetadata
    {
        public Task<IEnumerable<Camera>> GetAllCamerasAsync();
        public Task<IEnumerable<Camera>> SearchCamerasByNameAsync(string keyword, string brand, int? top = 10);
    }
}
