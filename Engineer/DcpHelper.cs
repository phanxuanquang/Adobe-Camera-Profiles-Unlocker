using System.Diagnostics;

namespace Engineer
{
    public static class DcpHelper
    {
        public static string AsXML(string dcpPath)
        {
            var xmlPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Path.GetFileName(dcpPath).Replace(".dcp", ".xml")}";

            var exeInfor = new ProcessStartInfo();
            exeInfor.FileName = @"Assets\dcpTool.exe";
            exeInfor.CreateNoWindow = true;
            exeInfor.Arguments = $"-d \"{dcpPath}\" \"{xmlPath}\"";

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();

            return xmlPath;
        }

        public static void AsDCP(string xmlPath, string dcpPath)
        {
            var exeInfor = new ProcessStartInfo();
            exeInfor.FileName = @"Assets\dcpTool.exe";
            exeInfor.Arguments = $"-c \"{xmlPath}\" \"{dcpPath}.dcp\"";
            exeInfor.CreateNoWindow = true;

            var executer = Process.Start(exeInfor);
            executer.WaitForExit();
        }
    }
}
