using log4net;
using System.IO;
using VIToACS.Configurations;
using VIToACS.Interfaces;

namespace VIToACS.Services
{
    public class FileStreamDocumentWriterService : IDocumentWriter
    {
        private readonly WriterConfig _config;
        private readonly ILog _logger;

        public FileStreamDocumentWriterService(WriterConfig config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }

        public void WriteScenesDocument(string fileName, string content)
        {
            var newFilename = _config.ScenesDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");
            WriteFile(_config.FileStream.ScenesPath, content, newFilename);
        }

        public void WriteThumbnailsDocument(string fileName, string content)
        {
            var newFilename = _config.ThumbnailsDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");
            WriteFile(_config.FileStream.ThumbnailsPath, content, newFilename);
        }

        private static void WriteFile(string path, string content, string newFilename)
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
        }
    }
}
