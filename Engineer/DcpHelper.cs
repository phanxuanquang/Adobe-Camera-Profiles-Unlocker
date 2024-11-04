using System.Diagnostics;

namespace Engineer
{
    public static class DcpHelper
    {
        private static readonly string DcpToolPath = @"Assets\dcpTool.exe";

        public static string AsXML(string dcpPath)
        {
            var xmlPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Path.GetFileName(dcpPath).Replace(".dcp", ".xml")}";

            var exeInfor = new ProcessStartInfo
            {
                FileName = DcpToolPath,
                CreateNoWindow = true,
                Arguments = $"-d \"{dcpPath}\" \"{xmlPath}\""
            };

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();

            return xmlPath;
        }

        public static void AsDCP(string xmlPath, string dcpPath)
        {
            var exeInfor = new ProcessStartInfo
            {
                FileName = DcpToolPath,
                Arguments = $"-c \"{xmlPath}\" \"{dcpPath}.dcp\"",
                CreateNoWindow = true
            };

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();
        }
    }
}
