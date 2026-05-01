using AdobeCameraProfilesUnlocker.Domain.Attributes;

namespace AdobeCameraProfilesUnlocker.Domain.Models.Enums;

public enum CameraProfileType : byte
{
    [Meta(@".dcp")]
    DCP,

    [Meta(@".xmp")]
    XMP
}