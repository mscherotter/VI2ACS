using log4net;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public class VideoIndexerFactory
    {
        public static IVideoIndexer CreateInstance(VideoIndexerConfig config, ReaderConfig readerConfig, ILog logger)
        {
            if (config == null)
            {
                throw new NullReferenceException();
            }

            return new VideoIndexerService(config, readerConfig, logger);
        }
    }
}
