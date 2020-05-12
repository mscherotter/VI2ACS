using log4net;
using System.Threading.Tasks;
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

        public async Task Run()
        {
            _logger.Info("Starting the Application.");

            _logger.Info("Creating the Scene Index.");
            _azureSearchService.CreateSceneIndex();

            _logger.Info("Creating the Thumbnail Index.");
            _azureSearchService.CreateThumbnailIndex();

            foreach (var parsedDocument in _insightsReaderService.ReadInsightsFiles())
            {
                _documentWriterService.WriteScenesDocument(parsedDocument.FileName, parsedDocument.ParsedScenesJson);
                _azureSearchService.UploadSceneDocuments(parsedDocument.Scenes);
                _documentWriterService.WriteThumbnailsDocument(parsedDocument.FileName, parsedDocument.ParsedThumbnailsJson);
                _azureSearchService.UploadThumbnailDocuments(parsedDocument.Thumbnails);
            }

            _logger.Info("Finishing the Application.");
        }
    }
}
