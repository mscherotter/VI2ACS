using log4net;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public static class InsightsReaderFactory
    {

        public static IInsightsReader CreateInstance(ReaderConfig readerConfig, WriterConfig writerConfig, ILog logger)
        {
            if (readerConfig == null)
            {
                throw new NullReferenceException();
            }
            if (writerConfig == null)
            {
                throw new NullReferenceException();
            }

            if (readerConfig.Type == "AzureBlob")
            {
                // If is it a AzureBlob type, it will add the Uri of the image as a property
                var thumbnailImageLocation = writerConfig.AzureBlob.ThumbnailsContainer;
                return new AzureBlobInsightsReaderService(readerConfig, logger, thumbnailImageLocation);
            }

            return new FileStreamInsightsReaderService(readerConfig, logger);
        }

    }
}
