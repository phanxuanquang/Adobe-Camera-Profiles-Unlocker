using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

class DcpConverter
{
    static void Main(string[] args)
    {
        string inputCameraModel = args.Length > 0 ? args[0] : "LEICA Q (Typ 116)";
        string outputCameraModel = args.Length > 1 ? args[1] : "LEICA M10";
        string inputDir = args.Length > 2 ? args[2] : "./input";
        string outputDir = args.Length > 3 ? args[3] : "./output";

        foreach (var existingProfile in Directory.EnumerateFiles(inputDir).Where(file => !file.StartsWith(".")))
        {
            string convertedProfile = ConvertProfileName(existingProfile, inputCameraModel, outputCameraModel);
            string existingDcpFilename = Path.Combine(inputDir, existingProfile);
            string xmlFilename = Path.Combine(outputDir, $"{convertedProfile}.xml");

            string decompileCommand = $"./bin/dcpTool -d \"{existingDcpFilename}\" \"{xmlFilename}\"";
            Console.WriteLine($"Decompiling {existingDcpFilename} into XML");
            ExecuteCommand(decompileCommand);

            Console.WriteLine($"Replacing camera model: {inputCameraModel} -> {outputCameraModel}");
            ReplaceCameraModel(xmlFilename, outputCameraModel);

            string convertedDcpFilename = Path.Combine(outputDir, $"{convertedProfile}.dcp");
            string recompileCommand = $"./bin/dcpTool -c \"{xmlFilename}\" \"{convertedDcpFilename}\"";
            Console.WriteLine($"Recompiling XML into {convertedDcpFilename}");
            ExecuteCommand(recompileCommand);

            File.Delete(xmlFilename);

            Console.WriteLine();
        }
    }

    static string ConvertProfileName(string profileName, string inputCameraModel, string outputCameraModel)
    {
        string escapedInputCameraModel = System.Text.RegularExpressions.Regex.Escape(inputCameraModel);
        return Path.GetFileNameWithoutExtension(profileName.Replace(inputCameraModel, outputCameraModel));
    }

    static void ReplaceCameraModel(string xmlProfileFilename, string outputCameraModel)
    {
        XDocument profileDoc = XDocument.Load(xmlProfileFilename);
        XElement element = profileDoc.Descendants("UniqueCameraModelRestriction").FirstOrDefault();
        if (element != null)
        {
            element.Value = outputCameraModel;
        }
        profileDoc.Save(xmlProfileFilename);
    }

    static void ExecuteCommand(string command)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"{command}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(processInfo))
        {
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.BeginOutputReadLine();
            process.ErrorDataReceived += (sender, e) => Console.WriteLine("ERROR: " + e.Data);
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
