using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Adobe_Camera_Profiles_Unlocker
{
    public enum Brand
    {
        [Display(Name = "Canon")]
        Canon = 1,

        [Display(Name = "Leica")]
        Leica = 2,

        [Display(Name = "Nikon")]
        Nikon = 3,

        [Display(Name = "Olympus")]
        Olympus = 4,

        [Display(Name = "Panasonic")]
        Panasonic = 5,

        [Display(Name = "Pentax")]
        Pentax = 8,

        [Display(Name = "Ricoh")]
        Ricoh = 9,

        [Display(Name = "Sony")]
        Sony = 10,
    }

    public static class EnumHelper
    {
        public static string GetName(Enum value)
        {
            Type type = value.GetType();
            MemberInfo[] memberInfo = type.GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.Name;
                }
            }
            return value.ToString();
        }
    }
}
