using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Adobe_Camera_Profiles_Unlocker_2._0
{
    public static class DirectoryHelper
    {
        public static string[] GetChilds(string folderPath)
        {
            return Directory.GetDirectories(folderPath);
        }
        public static List<string> GetDcpFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.dcp").ToList();
        }
    }
}
