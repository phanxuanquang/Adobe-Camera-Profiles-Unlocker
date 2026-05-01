using AAdobeCameraProfilesUnlocker.Domain.Attributes;

namespace AdobeCameraProfilesUnlocker.Domain.Models.Enums
{
    public enum CameraProfileType : byte
    {
        [RootProfileDirectory(@"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera")]
        DCP,

        [RootProfileDirectory(@"C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\Profiles\Camera")]
        XMP
    }
}
