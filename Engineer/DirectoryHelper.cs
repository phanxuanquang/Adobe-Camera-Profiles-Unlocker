using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer
{
    public static class DirectoryHelper
    {
        public static string[] GetChilds(string folderPath)
        {
            return Directory.GetDirectories(folderPath);
        }
        public static List<string> GetProfileFiles(string folderPath)
        {
            var dcpFiles = Directory.GetFiles(folderPath, "*.dcp").ToList();
            var xmpFiles = Directory.GetFiles(folderPath, "*.xmp").ToList();
            return dcpFiles.Concat(xmpFiles).ToList();
        }

        public static async Task<List<string>> GetFolders(string path, bool getDirectChildrenOnly = true)
        {
            if (getDirectChildrenOnly)
            {
                return await Task.Run(() => GetChilds(path).ToList());
            }

            var folders = new ConcurrentBag<string>();
            var stack = new ConcurrentStack<string>();

            stack.Push(path);

            while (!stack.IsEmpty)
            {
                if (stack.TryPop(out string currentPath))
                {
                    var subfolders = await Task.Run(() => Directory.GetDirectories(currentPath));

                    await Parallel.ForEachAsync(subfolders, async (folder, cancellationToken) =>
                    {
                        folders.Add(folder);
                        stack.Push(folder);
                    });
                }
            }

            return new List<string>(folders);
        }
    }
}
