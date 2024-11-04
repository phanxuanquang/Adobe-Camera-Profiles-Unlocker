using System.Diagnostics;
using System.Security.Principal;

namespace Engineer
{
    public static class WindowsHelper
    {
        public static bool IsUserAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void OpenFolderInExplorer(string folderPath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                }
            };
            process.Start();
        }
    }
}
