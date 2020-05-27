using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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

        public static string WriteFile(string path, byte[] bytes, string newFilename)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var newPath = Path.Combine(path, newFilename);
            File.WriteAllBytes(newPath, bytes);
            return newPath;
        }
    }
}
