using log4net;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public static class InsightsReaderFactory<T, T_Parser> where T_Parser : ISceneParser<T>, new()
    {

        public static IInsightsReader<T> CreateInstance(ReaderConfig readerConfig, WriterConfig writerConfig, ILog logger)
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
                // If it is a AzureBlob type, it will add the Uri of the image as a property
                var thumbnailImageLocation = writerConfig.AzureBlob.ThumbnailsContainer;
                return new AzureBlobInsightsReaderService<T, T_Parser>(readerConfig, logger, thumbnailImageLocation);
            }

            return new FileStreamInsightsReaderService<T, T_Parser>(readerConfig, logger);
        }

    }
}
