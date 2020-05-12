using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Models;
using VIToACS.Parsers;

namespace VIToACS.Services
{
    public class AzureBlobInsightsReaderService : IInsightsReader
    {

        private readonly ReaderConfig _config;
        private readonly ILog _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;

        public AzureBlobInsightsReaderService(ReaderConfig config, ILog logger)
        {
            _config = config;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_config.AzureBlob.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_config.AzureBlob.InsightsContainer);
        }

        public IEnumerable<ParsedDocument> ReadInsightsFiles()
        {

            foreach (BlobItem blobItem in _containerClient.GetBlobs())
            {
                var file = blobItem.Name;
                _logger.Info($"Reading the file { file }.");

                string downloadFilePath = DownloadBlob(file);

                IEnumerable<Scene> scenes = GetScenes(downloadFilePath);
                var scenesJson = JsonSerializer.Serialize(scenes, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

                IEnumerable<Thumbnail> thumbnails = GetThumbnails(downloadFilePath);
                var thumbnailsJson = JsonSerializer.Serialize(thumbnails, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

                File.Delete(downloadFilePath);

                yield return new ParsedDocument { 
                    FileName = file, 
                    ParsedScenesJson = scenesJson, 
                    ParsedThumbnailsJson = thumbnailsJson,
                    Scenes = scenes,
                    Thumbnails = thumbnails
                };
            }
        }

        private IEnumerable<Scene> GetScenes(string fileName)
        {
            _logger.Info($"Parsing scenes in the file { fileName }.");
            IEnumerable<Scene> scenes = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    var doc = JsonDocument.Parse(sr.ReadToEnd());
                    scenes = ScenesParser.GetScenes(doc);
                }
            }
            catch (FileNotFoundException)
            {
                _logger.Error($"File {fileName} was not found.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            return scenes;
        }


        private IEnumerable<Thumbnail> GetThumbnails(string fileName)
        {
            _logger.Info($"Parsing thumbnails in the file { fileName }.");

            IEnumerable<Thumbnail> thumbnails = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    var doc = JsonDocument.Parse(sr.ReadToEnd());
                    thumbnails = ThumbnailsParser.GetThumbnails(doc);
                }
                
            }
            catch (FileNotFoundException)
            {
                _logger.Error($"File {fileName} was not found.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            return thumbnails;
        }

        private string DownloadBlob(string fileName)
        {
            // Define temporary file to download
            string downloadPath = _config.AzureBlob.TempDownloadFilePath;

            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            string downloadFileName = Guid.NewGuid().ToString() + ".json";
            string downloadFilePath = Path.Combine(downloadPath, downloadFileName);

            // Get a reference to a blob
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);

            _logger.Info($"Downloading blob { fileName } to { downloadFilePath }");

            // Download the blob's contents and save it to a temporary file
            BlobDownloadInfo download = blobClient.Download();

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                download.Content.CopyTo(downloadFileStream);
                downloadFileStream.Close();
            }

            return downloadFilePath;
        }

    }
}
