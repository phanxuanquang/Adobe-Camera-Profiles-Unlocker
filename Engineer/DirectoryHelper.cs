using System;
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
        public static List<string> GetDcpFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.dcp").ToList();
        }
    }
}
