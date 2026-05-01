using AdobeCameraProfilesUnlocker.Core.Models;
using AdobeCameraProfilesUnlocker.Core.Models.Enums;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace AdobeCameraProfilesUnlocker.Playground
{
    internal class Program
    {
        static string AdobeStandardProfiles = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Adobe Standard";
        static string VariantProfileFolders = @"C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera";
        static string SpecialVariantProfileFolders = @"C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\Profiles\Camera";

        static async Task Main(string[] args)
        {
            var cameraMetadatas = LoadCameraMetadatas();

            var cameraBrands = cameraMetadatas
                .Select(x => x.Brand)
                .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

            var cameras = cameraMetadatas
                .Select(x => new Camera
                {
                    CodeName = x.Name,
                    BrandId = cameraBrands.First(b => b.Name.Equals(x.Brand, StringComparison.OrdinalIgnoreCase)).Id,
                })
                .ToList();

            var cameraProfiles = new List<CameraProfile>();
            foreach (var camera in cameras)
            {
                var metadata = cameraMetadatas.First(x => x.Name.Equals(camera.CodeName, StringComparison.OrdinalIgnoreCase));
                cameraProfiles.AddRange(metadata.Profiles
                    .Select(profile => new CameraProfile
                    {
                        Name = profile.Name,
                        ExtensionId = profile.IsDcpFile ? CameraProfileExtension.DCP : CameraProfileExtension.XMP,
                        FilePath = profile.FilePath,
                        CameraId = camera.Id
                    }));
            }
        }

        public static List<CameraMetadata> LoadCameraMetadatas()
        {
            var cameras = EnumerateFilesSafe(AdobeStandardProfiles)
                .Select(ParseCamera)
                .DistinctBy(x => x.CameraName)
                .ToFrozenDictionary(x => x.CameraName, x => x.Brand);

            var bag = new ConcurrentBag<CameraMetadata>();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount / 2)
            };

            Parallel.ForEach(cameras, options, camera =>
            {
                var standardDir = Path.Combine(VariantProfileFolders, camera.Key);
                var altDir = Path.Combine(SpecialVariantProfileFolders, camera.Value, camera.Key);

                var profiles = EnumerateFilesSafe(standardDir)
                    .Concat(EnumerateFilesSafe(altDir))
                    .Select(profilePath =>
                    {
                        var name = Path.GetFileNameWithoutExtension(profilePath)
                            .Replace(camera.Key, string.Empty)
                            .Replace("Camera", string.Empty)
                            .Trim();

                        return new ProfileMetadata
                        {
                            Name = name,
                            FilePath = profilePath,
                            IsDcpFile = profilePath.EndsWith(".dcp", StringComparison.OrdinalIgnoreCase)
                        };
                    })
                    .ToList();

                if (profiles.Count == 0)
                    return;

                bag.Add(new CameraMetadata
                {
                    Name = camera.Key,
                    Brand = camera.Value,
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

    public class CameraMetadata
    {
        public required string Name { get; set; }
        public required string Brand { get; set; }
        public required List<ProfileMetadata> Profiles { get; set; }
    }

    public class Profile
    {
        public required string Name { get; set; }
        public required string FilePath { get; set; }
        public required bool IsDcpFile { get; set; }
    }
}
