using log4net;
using System;
using System.Collections.Generic;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Models;

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

        public IEnumerable<ParsedDocument> ReadInsightsFiles()
        {
            throw new NotImplementedException();
        }

    }
}
