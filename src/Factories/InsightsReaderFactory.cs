using log4net;
using System;
using VIToACS.Interfaces;
using VIToACS.Configurations;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public static class InsightsReaderFactory
    {

        public static IInsightsReader CreateInstance(ReaderConfig config, ILog logger)
        {
            if (config == null)
            {
                throw new NullReferenceException();
            }

            if (config.Type == "AzureBlob")
            {
                return new AzureBlobInsightsReaderService(config, logger);
            }

            return new FileStreamInsightsReaderService(config, logger);
        }

    }
}
