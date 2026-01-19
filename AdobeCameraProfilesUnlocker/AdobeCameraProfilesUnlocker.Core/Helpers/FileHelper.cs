using System.Text;

namespace AdobeCameraProfilesUnlocker.Core.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> ReadAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file was not found.", filePath);
            }

            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }
        public static async Task OverwriteAsync(string filePath, string content)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file was not found.", filePath);
            }

            using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            await streamWriter.WriteAsync(content);
        }
        public static async Task MoveAsync(string oldFilePath, string newFilePath)
        {
            if (!File.Exists(oldFilePath))
            {
                throw new FileNotFoundException("The specified file was not found.", oldFilePath);
            }

            var targetDirectory = Path.GetDirectoryName(newFilePath);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory!);
            }

            await Task.Run(() => File.Move(oldFilePath, newFilePath));
        }
    }
}
