namespace Engineer
{
    public static class ContentModifier
    {
        private static readonly string AuthorCredit = "© 2024 Phan Xuan Quang / Github: @phanxuanquang";
        public static void ModifyXmp(string xmlPath, string outputCameraModel)
        {
            var lines = File.ReadAllLines(xmlPath);

            Parallel.For(0, lines.Length, i =>
            {
                if (lines[i].Contains("crs:Copyright="))
                {
                    lines[i] = $"crs:Copyright=\"{AuthorCredit}\"";
                }

                if (lines[i].Contains("crs:CameraModelRestriction="))
                {
                    lines[i] = $"crs:CameraModelRestriction=\"{outputCameraModel.Trim()}\"";
                }

                if (lines[i].Contains("crs:CameraProfile="))
                {
                    lines[i] = $"crs:CameraProfile=\"Camera Standard\"";
                }
            });

            File.WriteAllLines(xmlPath, lines);
        }

        public static void ModifyXml(string xmlPath, string cameraModel, string outputCameraModel)
        {
            var lines = File.ReadAllLines(xmlPath);

            Parallel.For(0, lines.Length, i =>
            {
                if (lines[i].Contains("<ProfileName>"))
                {
                    lines[i] = lines[i].Replace("Camera", $"{cameraModel.Trim()}:");
                }

                if (lines[i].Contains("Copyright"))
                {
                    lines[i] = lines[i].Replace(lines[i], $"<Copyright>{AuthorCredit}</Copyright>");
                }

                if (lines[i].Contains("<ProfileCalibrationSignature>"))
                {
                    lines[i] = lines[i].Replace("com.adobe", "Phan Xuan Quang");
                }

                if (lines[i].Contains("<UniqueCameraModelRestriction>"))
                {
                    lines[i] = lines[i].Replace(lines[i], $"<UniqueCameraModelRestriction>{outputCameraModel.Trim()}</UniqueCameraModelRestriction>");
                }
            });

            File.WriteAllLines(xmlPath, lines);
        }
    }
}
