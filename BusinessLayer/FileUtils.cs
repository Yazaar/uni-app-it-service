using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    internal static class FileUtils
    {
        internal static void MoveFileToDir(string filePath, string newDir)
        {
            if (!File.Exists(filePath)) return;

            Directory.CreateDirectory(newDir);

            int lastSlashIndex = filePath.LastIndexOf('\\') + 1;
            string currentFile = filePath.Substring(lastSlashIndex);

            string[] currentFileParts = currentFile.Split('.', 2);
            string filename = currentFileParts[0];

            string fileext;

            if (currentFileParts.Length == 2) fileext = $".{currentFileParts[1]}";
            else fileext = string.Empty;

            string currentTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            try { File.Move(filePath, $@"{newDir}\{filename}-{currentTime}{fileext}"); }
            catch { /* Filen är förmodligen fortfarande öppen, ignorera flyttning av fil */ }
        }
    }
}
