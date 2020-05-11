using log4net;
using VIToACS.Interfaces;

namespace VIToACS
{
    public class AppHost
    {
        private readonly IAzureSearch _azureSearchService;
        private readonly IInsightsReader _insightsReaderService;
        private readonly IDocumentWriter _documentWriterService;
        private readonly ILog _logger;

        public AppHost(IAzureSearch azureSearchService, IInsightsReader insightsReaderService, IDocumentWriter documentWriterService, ILog logger)
        {
            _azureSearchService = azureSearchService;
            _insightsReaderService = insightsReaderService;
            _documentWriterService = documentWriterService;
            _logger = logger;
        }

        public void Run()
        {
            _logger.Info("Starting the Application.");

            _logger.Info("Creating the Scene Index.");
            _azureSearchService.CreateSceneIndex();
        }
    }
}
