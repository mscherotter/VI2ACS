using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using VIToACS.Interfaces;
using VIToACS.Configurations;
using VIToACS.Services;

namespace VIToACS.Factories
{
    public static class DocumentWriterFactory
    {
        public static IDocumentWriter CreateInstance(WriterConfig config, ILog logger)
        {
            if (config == null)
                throw new NullReferenceException();

            if (config.Type == "AzureBlob")
                return new AzureBlobDocumentWriterService(config, logger);

            return new FileStreamDocumentWriterService(config, logger);
        }

    }
}
