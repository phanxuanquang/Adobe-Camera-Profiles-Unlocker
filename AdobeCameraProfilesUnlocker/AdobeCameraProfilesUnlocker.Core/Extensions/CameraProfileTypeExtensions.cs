using AdobeCameraProfilesUnlocker.Core.Attributes;
using AdobeCameraProfilesUnlocker.Core.Models.Enums;

namespace AdobeCameraProfilesUnlocker.Core.Extensions
{
    public static class CameraProfileTypeExtensions
    {
        public static string GetRootDirectory(this CameraProfileExtension value)
        {
            var memberInfo = value.GetType().GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(RootProfileDirectoryAttribute), false);
            var directoryAttribute = (RootProfileDirectoryAttribute)attributes[0];
            return directoryAttribute.Directory;
        }
    }
}
