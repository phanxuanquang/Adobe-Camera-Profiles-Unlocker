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

        public static void UpdateXMLContent(string xmlPath, string brand, string outputCameraModel)
        {
            var lines = File.ReadAllLines(xmlPath);

            Parallel.For(0, lines.Length, i =>
            {
                if (lines[i].Contains("<ProfileName>"))
                {
                    lines[i] = lines[i].Replace("Camera", $"{brand}:");
                }

                if (lines[i].Contains("Copyright"))
                {
                    lines[i] = lines[i].Replace(lines[i], "<Copyright>© 2024 Phan Xuan Quang / Github: @phanxuanquang</Copyright>");
                }

                if (lines[i].Contains("<ProfileCalibrationSignature>"))
                {
                    lines[i] = lines[i].Replace("com.adobe", "Phan Xuan Quang");
                }

                if (lines[i].Contains("<UniqueCameraModelRestriction>"))
                {
                    lines[i] = lines[i].Replace(lines[i], $"<UniqueCameraModelRestriction>{outputCameraModel}</UniqueCameraModelRestriction>");
                }
            });

            File.WriteAllLines(xmlPath, lines);
        }
    }
}
