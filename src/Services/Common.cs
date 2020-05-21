using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VIToACS.Services
{
    public static class Common
    {
        public static string WriteFile(string path, string content, string newFilename)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            FileStream outputStream;
            StreamWriter writer;
            var newPath = Path.Combine(path, newFilename);
            outputStream = new FileStream(newPath, FileMode.Create, FileAccess.Write);
            using (writer = new StreamWriter(outputStream))
            {
                writer.Write(content);
            }
            return newPath;
        }
    }
}
