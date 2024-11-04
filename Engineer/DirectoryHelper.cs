using System.Collections.Concurrent;

namespace Engineer
{
    public static class DirectoryHelper
    {
        public static async Task<List<string>> GetProfiles(string folderPath)
        {
            var profiles = new ConcurrentBag<string>();

            await Task.WhenAll(
                Task.Run(() => Parallel.ForEach(Directory.EnumerateFiles(folderPath, "*.dcp"), file =>
                {
                    profiles.Add(file);
                })),
                Task.Run(() => Parallel.ForEach(Directory.EnumerateFiles(folderPath, "*.xmp"), file =>
                {
                    profiles.Add(file);
                }))
            );

            return new List<string>(profiles);
        }

        public static async Task<List<string>> GetFolders(string path, bool getDirectChildrenOnly = true)
        {
            if (!Directory.EnumerateFileSystemEntries(path).GetEnumerator().MoveNext())
            {
                return [];
            }

            if (getDirectChildrenOnly)
            {
                return await Task.Run(() => Directory.GetDirectories(path).ToList());
            }

            var folders = new ConcurrentBag<string>();
            var stack = new ConcurrentStack<string>();

            stack.Push(path);

            while (!stack.IsEmpty)
            {
                if (stack.TryPop(out string currentPath))
                {
                    var subfolders = await Task.Run(() => Directory.GetDirectories(currentPath));

                    await Parallel.ForEachAsync(subfolders, (folder, cancellationToken) =>
                    {
                        folders.Add(folder);
                        stack.Push(folder);
                        return new ValueTask();
                    });
                }
            }

            return new List<string>(folders);
        }

        public static bool IsDirectoryExist(string path)
        {
            return Directory.Exists(path);
        }

        public static void CreateDirectoryIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
