namespace AdobeCameraProfilesUnlocker.Infrastructure.Helpers;

internal static class IOHelper
{
    internal static IEnumerable<string> EnumerateFilesSafe(string path)
    {
        if (!Directory.Exists(path))
            yield break;

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            yield return file;
    }
}