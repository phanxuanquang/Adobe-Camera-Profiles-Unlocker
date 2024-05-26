using System.ComponentModel.DataAnnotations;

namespace Adobe_Camera_Profiles_Unlocker
{
    public enum ConvertOption
    {
        [Display(Name = "-c")]
        DCP = 1,

        [Display(Name = "-d")]
        XML = 2,
    }
}
