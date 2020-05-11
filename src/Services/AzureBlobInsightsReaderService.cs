using log4net;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;

namespace VIToACS.Services
{
    public class AzureBlobInsightsReaderService : IInsightsReader
    {

        private readonly ReaderConfig _config;
        private readonly ILog _logger;

        public AzureBlobInsightsReaderService(ReaderConfig config, ILog logger)
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
