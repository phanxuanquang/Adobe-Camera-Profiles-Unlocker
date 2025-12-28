using System.Collections.Concurrent;

namespace AdobeCameraProfilesUnlocker.Playground
{
    internal class Program
    {
        static string AdobeStandardProfiles = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Adobe Standard";
        static string VariantProfileFolders = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";
        static string SpecialVariantProfileFolders = @"C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\Profiles\Camera";

        static async Task Main(string[] args)
        {
            var camera = LoadCameraMetadatas();

            var brands = camera
                .Select(x => x.Brand)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var x = 1;
        }

        public static List<Camera> LoadCameraMetadatas()
        {
            var cameras = EnumerateFilesSafe(AdobeStandardProfiles)
                .Select(ParseCamera)
                .DistinctBy(x => x.CameraName)
                .ToList();

            var bag = new ConcurrentBag<Camera>();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount / 2)
            };

            Parallel.ForEach(cameras, options, camera =>
            {
                var standardDir = Path.Combine(VariantProfileFolders, camera.CameraName);
                var altDir = Path.Combine(SpecialVariantProfileFolders, camera.Brand, camera.CameraName);

                var profiles = EnumerateFilesSafe(standardDir)
                    .Concat(EnumerateFilesSafe(altDir))
                    .Select(profilePath =>
                    {
                        var name = Path.GetFileNameWithoutExtension(profilePath)
                            .Replace(camera.CameraName, string.Empty)
                            .Replace("Camera", string.Empty)
                            .Trim();

                        return new Profile
                        {
                            Name = name,
                            FilePath = profilePath,
                            IsDcpFile = profilePath.EndsWith(".dcp", StringComparison.OrdinalIgnoreCase)
                        };
                    })
                    .ToList();

                if (profiles.Count == 0)
                    return;

                bag.Add(new Camera
                {
                    Name = camera.CameraName,
                    Brand = camera.Brand,
                    Profiles = profiles
                });
            });

            return bag.OrderBy(x => x.Name).ToList();
        }

        public static IEnumerable<string> EnumerateFilesSafe(string path)
        {
            if (!Directory.Exists(path))
                yield break;

            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                yield return file;
        }

        private static (string CameraName, string Brand) ParseCamera(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);

            name = name
                .Replace(" Adobe Standard", string.Empty)
                .Replace(" Adobe_Standard", string.Empty)
                .Replace(" Camera Default", string.Empty)
                .Trim();

            var spaceIndex = name.IndexOf(' ');
            var brand = spaceIndex < 0 ? name : name[..spaceIndex];

            return (name, brand);
        }
    }

    public class Camera
    {
        public required string Name { get; set; }
        public required string Brand { get; set; }
        public required List<Profile> Profiles { get; set; }
    }

    public class Profile 
    {
        public required string Name { get; set; }
        public required string FilePath { get; set; }
        public required bool IsDcpFile { get; set; }
    }
}
