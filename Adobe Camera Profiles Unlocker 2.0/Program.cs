using System;
using System.Windows.Forms;

namespace Adobe_Camera_Profiles_Unlocker_2._0
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindows());
        }
    }
}
