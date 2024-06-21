using System.Diagnostics;
using System.Security.Principal;

namespace Adobe_Camera_Profiles_Unlocker_2._0
{
    public static class GeneralHelper
    {
        public static bool IsUserAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                CreateNoWindow = true
            });
        }
    }
}
