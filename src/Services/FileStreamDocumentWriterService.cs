using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using VIToACS.Interfaces;
using VIToACS.Configurations;

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
            Console.WriteLine(fileName, content);
        }

        public void WriteThumbnailsDocument(string fileName, string content)
        {
            Console.WriteLine(fileName, content);
        }
    }
}
