using log4net;
using System;
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
            if (config == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _logger = logger;
        }

        public void WriteScenesDocument(string fileName, string content)
        {
            var newFilename = _config.ScenesDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");
            Common.WriteFile(_config.FileStream.ScenesPath, content, newFilename);
        }

        public void WriteThumbnailsDocument(string fileName, string content)
        {
            var newFilename = _config.ThumbnailsDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");
            Common.WriteFile(_config.FileStream.ThumbnailsPath, content, newFilename);
        }
    }
}
