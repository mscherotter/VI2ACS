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
    public class AzureBlobInsightsReaderService<T, T_Parser> : IInsightsReader<T> where T_Parser : ISceneParser<T>, new()
    {

        private readonly ReaderConfig _config;
        private readonly ILog _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _insightsContainerClient;
        private readonly BlobContainerClient _failedInsightsContainerClient;
        private readonly string _thumbnailImageLocation;


        public AzureBlobInsightsReaderService(ReaderConfig config, ILog logger, string thumbnailImageLocation)
        {
            if (config == null || logger == null)
            {
                throw new NullReferenceException();
            }

            _config = config;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_config.AzureBlob.ConnectionString);
            _insightsContainerClient = _blobServiceClient.GetBlobContainerClient(_config.AzureBlob.InsightsContainer);
            _insightsContainerClient.CreateIfNotExists();
            _failedInsightsContainerClient = _blobServiceClient.GetBlobContainerClient(_config.AzureBlob.FailedInsightsContainer);
            _failedInsightsContainerClient.CreateIfNotExists();
            _thumbnailImageLocation = thumbnailImageLocation;
        }

        public void AddNewFile(string fileName, string content)
        {
            _logger.Info($"Adding the file { fileName }.");

            try
            {
                var newPath = Common.WriteFile(_config.AzureBlob.TempDownloadFilePath, content, fileName);

                // Get a reference to a blob
                BlobClient blobClient = _insightsContainerClient.GetBlobClient(fileName);
                using FileStream uploadFileStream = File.OpenRead(newPath);
                blobClient.Upload(uploadFileStream, true);
                uploadFileStream.Close();
                File.Delete(newPath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            catch (RequestFailedException ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        public IEnumerable<ParsedDocument<T>> ReadInsightsFiles()
        {

            foreach (BlobItem blobItem in _insightsContainerClient.GetBlobs())
            {
                string scenesJson = string.Empty;
                string thumbnailsJson = string.Empty;

                var file = blobItem.Name;
                _logger.Debug($"Reading the file { file }.");

                string downloadFilePath = DownloadBlob(file);

                IEnumerable<T> scenes = GetScenes(downloadFilePath);
                if (scenes == null)
                {
                    _logger.Warn($"It was not possible to extract the scenes from the file { file }.");
                    // Upload failed blob to the failed insights container
                    BlobClient blobClient = _failedInsightsContainerClient.GetBlobClient(file);
                    using FileStream uploadFileStream = File.OpenRead(downloadFilePath);
                    blobClient.Upload(uploadFileStream, true);
                    uploadFileStream.Close();
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
                    // Upload failed blob to the failed insights container
                    BlobClient blobClient = _failedInsightsContainerClient.GetBlobClient(file);
                    using FileStream uploadFileStream = File.OpenRead(downloadFilePath);
                    blobClient.Upload(uploadFileStream, true);
                    uploadFileStream.Close();
                }
                else
                {
                    // Update the Uri for each thumbnail
                    foreach (var thumbnail in thumbnails)
                    {
                        thumbnail.Uri = $"https://{ _blobServiceClient.AccountName }.blob.core.windows.net/{ _thumbnailImageLocation }/thumbnail_{ Path.GetFileNameWithoutExtension(file) }_{ thumbnail.Id }.jpeg";
                    }

                    thumbnailsJson = JsonSerializer.Serialize(thumbnails, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
                    _logger.Debug($"The file { file } has { thumbnails.Count() } thumbnails.");
                }

                File.Delete(downloadFilePath);

                yield return new ParsedDocument<T>
                {
                    FileName = file,
                    ParsedScenesJson = scenesJson,
                    ParsedThumbnailsJson = thumbnailsJson,
                    Scenes = scenes,
                    Thumbnails = thumbnails
                };
            }
        }

        private IEnumerable<T> GetScenes(string fileName)
        {
            _logger.Info($"Parsing scenes from the file { fileName }.");
            IEnumerable<T> scenes = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    var doc = JsonDocument.Parse(sr.ReadToEnd());
                    var parser = new T_Parser();
                    
                    scenes = parser.GetScenes(doc);
                }
            }
            catch (FileNotFoundException)
            {
                _logger.Error($"File {fileName} was not found.");
                return null;
            }
            catch (JsonException)
            {
                _logger.Error($"Error parsing the JSON file {fileName}.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
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
                return null;
            }
            catch (JsonException)
            {
                _logger.Error($"Error parsing the JSON file {fileName}.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
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
            BlobClient blobClient = _insightsContainerClient.GetBlobClient(fileName);

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
