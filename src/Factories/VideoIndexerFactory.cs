using log4net;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public class VideoIndexerFactory<T>
    {
        public static IVideoIndexer<T> CreateInstance(VideoIndexerConfig config, ReaderConfig readerConfig, ILog logger)
        {
            if (config == null)
            {
                return null;
            }

            return new VideoIndexerService<T>(config, readerConfig, logger);
        }
    }
}
