using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (config == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_config.AzureBlob.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_config.AzureBlob.InsightsContainer);
            _containerClient.CreateIfNotExists();
        }

        public IEnumerable<ParsedDocument> ReadInsightsFiles()
        {

            foreach (BlobItem blobItem in _containerClient.GetBlobs())
            {
                string scenesJson = string.Empty;
                string thumbnailsJson = string.Empty;

                var file = blobItem.Name;
                _logger.Debug($"Reading the file { file }.");

                string downloadFilePath = DownloadBlob(file);

                IEnumerable<Scene> scenes = GetScenes(downloadFilePath);
                if (scenes == null)
                {
                    _logger.Warn($"It was not possible to extract the scenes from the file { file }.");
                }
                else
                {
                    scenesJson = JsonSerializer.Serialize(scenes, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
                    _logger.Debug($"The file { file } has { scenes.Count() } scenes.");
                }

                IEnumerable<Thumbnail> thumbnails = GetThumbnails(downloadFilePath);
                if (thumbnails == null)
                {
                    _logger.Warn($"It was not possible to thumbnails the scenes from the file { file }.");
                }
                else
                {
                    thumbnailsJson = JsonSerializer.Serialize(thumbnails, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
                    _logger.Debug($"The file { file } has { thumbnails.Count() } thumbnails.");
                }

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
            _logger.Info($"Parsing scenes from the file { fileName }.");
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
            catch (JsonException)
            {
                _logger.Error($"Error parsing the JSON file {fileName}.");
                return null;
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
            _logger.Info($"Parsing thumbnails from the file { fileName }.");

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
            catch (JsonException)
            {
                _logger.Error($"Error parsing the JSON file {fileName}.");
                return null;
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
