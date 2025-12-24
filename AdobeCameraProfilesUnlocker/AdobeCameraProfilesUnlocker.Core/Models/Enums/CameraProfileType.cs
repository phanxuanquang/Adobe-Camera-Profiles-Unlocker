using AdobeCameraProfilesUnlocker.Core.Attributes;

namespace AdobeCameraProfilesUnlocker.Core.Models.Enums
{
    public enum CameraProfileType
    {
        [RootProfileDirectory(@"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera")]
        DCP,

        [RootProfileDirectory(@"C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\Profiles\Camera")]
        XMP
    }
}
