using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
