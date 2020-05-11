using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using VIToACS.Configurations;
using VIToACS.Factories;

namespace VIToACS
{
    class Program
    {
        public static void Main()
        {
            // Load the log4net configuration
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // Read the app settings
            var builder = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json");

            var config = builder.Build();
            var azureSearchConfig = config.GetSection("azureSearch").Get<AzureSearchConfig>();
            var readerConfig = config.GetSection("reader").Get<ReaderConfig>();
            var writerConfig = config.GetSection("writer").Get<WriterConfig>();
            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            // Build the services
            var serviceProvider = new ServiceCollection()
                .AddSingleton<AppHost, AppHost>()
                .AddSingleton(AzureSearchFactory.CreateInstance(azureSearchConfig, logger))
                .AddSingleton(InsightsReaderFactory.CreateInstance(readerConfig, logger))
                .AddSingleton(DocumentWriterFactory.CreateInstance(writerConfig, logger))
                .AddSingleton(logger)
                .BuildServiceProvider();

            serviceProvider.GetService<AppHost>().Run();

        }
    }
}
