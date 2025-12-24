
using AdobeCameraProfilesUnlocker.Core.Models;

namespace AdobeCameraProfilesUnlocker.Interfaces
{
    public interface ICameraProfile
    {
        public Task<List<Camera>> SearchProfilesByNameAsync(string keyword, int? top = 10);
        public Task<List<Camera>> GetAllProfilesAsync();
    }
}
