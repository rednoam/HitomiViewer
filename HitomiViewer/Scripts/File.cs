using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    partial class File2
    {
        public static double GetFolderByte(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            return FolderByte;
        }
        public static double GetSizePerPage(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            double SizePerPage = FolderByte / info.GetFiles().Length;
            return SizePerPage;
        }
        public static string[] GetImages(string dir)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".lock" };
            return Directory.GetFiles(dir).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort();
        }
    }
}
