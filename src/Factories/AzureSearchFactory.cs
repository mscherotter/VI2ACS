using log4net;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public class AzureSearchFactory
    {
        public static IAzureSearch CreateInstance(AzureSearchConfig config, ILog logger)
        {
            if (config == null)
            {
                throw new NullReferenceException();
            }

            return new AzureSearchService(config, logger);
        }
    }
}
