using AdobeCameraProfilesUnlocker.Core.Models.Enums;

namespace AdobeCameraProfilesUnlocker.Core.Models
{
    public class Camera
    {
        public required string Name { get; set; }
        public required CameraProfileType ProfileType { get; set; }
        public List<string> Profiles { get; set; } = [];
    }
}
