namespace Engineer
{
    public static class FileUpdater
    {
        public static void ModifyXMPContent(string xmlPath, string brand, string outputCameraModel)
        {
            var lines = File.ReadAllLines(xmlPath);

            Parallel.For(0, lines.Length, i =>
            {
                if (lines[i].Contains("crs:Copyright="))
                {
                    lines[i] = "crs:Copyright=\"© 2024 Phan Xuan Quang / Github: @phanxuanquang\"";
                }

                if (lines[i].Contains("crs:CameraModelRestriction="))
                {
                    lines[i] = $"crs:CameraModelRestriction=\"{outputCameraModel}\"";
                }

                if (lines[i].Contains("crs:CameraProfile="))
                {
                    lines[i] = $"crs:CameraProfile=\"Camera Standard\"";
                }
            });

            File.WriteAllLines(xmlPath, lines);
        }

        public static void ModifyXMLContent(string xmlPath, string brand, string outputCameraModel)
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
