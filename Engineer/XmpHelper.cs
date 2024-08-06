using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer
{
    public static class XmpHelper
    {
        public static void UpdateXMPContent(string xmlPath, string brand, string outputCameraModel)
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
    }
}
