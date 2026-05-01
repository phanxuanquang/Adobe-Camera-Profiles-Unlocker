
using AdobeCameraProfilesUnlocker.Core.Models;

namespace AdobeCameraProfilesUnlocker.Interfaces
{
    public interface ICamera
    {
        public Task<List<Camera>> SearchCamerasAsync(string keyword, int? brandId = null, int top = 10);
        public Task<Camera> GetAsync(int cameraId);
        public Task<List<CameraProfile>> GetProfilesAsync(int cameraId);
        public Task<List<Camera>> GetAllAsync();
    }
}
