using log4net;
using VIToACS.Interfaces;

namespace VIToACS
{
    public class AppHost
    {
        private readonly IVideoIndexer _videoIndexerService;
        private readonly IAzureSearch _azureSearchService;
        private readonly IInsightsReader _insightsReaderService;
        private readonly IDocumentWriter _documentWriterService;
        private readonly ILog _logger;

        public AppHost(IVideoIndexer videoIndexerService, IAzureSearch azureSearchService, IInsightsReader insightsReaderService, IDocumentWriter documentWriterService, ILog logger)
        {
            _videoIndexerService = videoIndexerService;
            _azureSearchService = azureSearchService;
            _insightsReaderService = insightsReaderService;
            _documentWriterService = documentWriterService;
            _logger = logger;
        }

        public void Run()
        {
            _logger.Info("Starting the Application.");

            if (_videoIndexerService.IsEnabled())
            {
                // List the videos from Video Library and extract the insights file
                // The new file is add to a location known by the reader service
                _logger.Info("Reading videos from Video Indexer library.");
                var done = false;
                var skip = 0;
                while (!done)
                {
                    var mediaAssets = _videoIndexerService.ListVideosAsync(skip).GetAwaiter().GetResult();
                    if (mediaAssets != null)
                    {
                        if (mediaAssets.Results != null)
                        {
                            foreach (var media in mediaAssets.Results)
                            {
                                _logger.Debug($"Reading and saving insights from the Video: { media.Name } with the Id: { media.Id }.");
                                // Add a new file to a location know by the reader service
                                _videoIndexerService.AddNewInsightsFileToReaderAsync(_insightsReaderService, media);
                            }
                            done = mediaAssets.NextPage.Done;
                            skip = mediaAssets.NextPage.Skip;
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }
            }

            // Create Scence Index in the Azure Cognitive Search
            _logger.Info("Creating the Scene Index.");
            _azureSearchService.CreateSceneIndex();

            // Create Thumbnail Index in the Azure Cognitive Search
            _logger.Info("Creating the Thumbnail Index.");
            _azureSearchService.CreateThumbnailIndex();


            // Parse all insights files
            foreach (var parsedDocument in _insightsReaderService.ReadInsightsFiles())
            {
                if (parsedDocument != null)
                {
                    if (parsedDocument.Scenes != null)
                    {
                        // Scenes
                        _documentWriterService.WriteScenesDocument(parsedDocument.FileName, parsedDocument.ParsedScenesJson);
                        _logger.Info($"Uploading scenes from the file { parsedDocument.FileName } to Azure Cognitive Search.");
                        _azureSearchService.UploadSceneDocuments(parsedDocument.Scenes);
                        _logger.Debug($"The scenes from the file { parsedDocument.FileName } have been parsed and uploaded.");
                    }
                    if (parsedDocument.Thumbnails != null)
                    {
                        // Thumbnails
                        _documentWriterService.WriteThumbnailsDocument(parsedDocument.FileName, parsedDocument.ParsedThumbnailsJson);
                        _logger.Info($"Uploading thumbnails from the file { parsedDocument.FileName } to Azure Cognitive Search.");
                        _azureSearchService.UploadThumbnailDocuments(parsedDocument.Thumbnails);
                        _logger.Debug($"The thumbnails from the file { parsedDocument.FileName } have been parsed and uploaded.");
                    }
                }
            }
            _logger.Info("Finishing the Application.");
        }
    }
}
