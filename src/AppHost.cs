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

        public void Run()
        {
            _logger.Info("Starting the Application.");

            _logger.Info("Creating the Scene Index.");
            _azureSearchService.CreateSceneIndex();

            _logger.Info("Creating the Thumbnail Index.");
            _azureSearchService.CreateThumbnailIndex();


            foreach (var parsedDocument in _insightsReaderService.ReadInsightsFiles())
            {
                // Scenes
                _documentWriterService.WriteScenesDocument(parsedDocument.FileName, parsedDocument.ParsedScenesJson);
                _logger.Info($"Uploading scenes from the file { parsedDocument.FileName }.");
                _azureSearchService.UploadSceneDocuments(parsedDocument.Scenes);
                
                // Thumbnails
                _documentWriterService.WriteThumbnailsDocument(parsedDocument.FileName, parsedDocument.ParsedThumbnailsJson);
                _logger.Info($"Uploading thumbnails from the file { parsedDocument.FileName }.");
                _azureSearchService.UploadThumbnailDocuments(parsedDocument.Thumbnails);

                _logger.Debug($"The file { parsedDocument.FileName } has been parsed and uploaded.");
            }

            _logger.Info("Finishing the Application.");
        }
    }
}
