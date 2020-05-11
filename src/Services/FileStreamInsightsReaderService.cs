using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using VIToACS.Interfaces;
using VIToACS.Configurations;

namespace VIToACS.Services
{
    public class FileStreamInsightsReaderService : IInsightsReader
    {
        private readonly ReaderConfig _config;
        private readonly ILog _logger;


        public FileStreamInsightsReaderService(ReaderConfig config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }

        public void ReadInsightsFile(string fileName)
        {
            Console.WriteLine(fileName);
        }
    }
}
