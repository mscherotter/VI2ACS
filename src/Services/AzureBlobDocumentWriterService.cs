using Azure;
using Azure.Storage.Blobs;
using log4net;
using System;
using System.IO;
using VIToACS.Configurations;
using VIToACS.Interfaces;

namespace VIToACS.Services
{
    public class AzureBlobDocumentWriterService : IDocumentWriter
    {
        private readonly WriterConfig _config;
        private readonly ILog _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _scenesContainerClient;
        private readonly BlobContainerClient _thumbnailsContainerClient;

        public AzureBlobDocumentWriterService(WriterConfig config, ILog logger)
        {
            if (config == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_config.AzureBlob.ConnectionString);
            _scenesContainerClient = _blobServiceClient.GetBlobContainerClient(_config.AzureBlob.ScenesContainer);
            _thumbnailsContainerClient = _blobServiceClient.GetBlobContainerClient(_config.AzureBlob.ThumbnailsContainer);
        }

        public void WriteScenesDocument(string fileName, string content)
        {
            var newFilename = _config.ScenesDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");

            try
            {
                var newPath = WriteFile(_config.AzureBlob.TempUploadFilePath, content, newFilename);

                // Get a reference to a blob
                BlobClient blobClient = _scenesContainerClient.GetBlobClient(newFilename);
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

        public void WriteThumbnailsDocument(string fileName, string content)
        {
            var newFilename = _config.ThumbnailsDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");
            
            try
            {
                var newPath = WriteFile(_config.AzureBlob.TempUploadFilePath, content, newFilename);

                // Get a reference to a blob
                BlobClient blobClient = _thumbnailsContainerClient.GetBlobClient(newFilename);
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

        private static string WriteFile(string path, string content, string newFilename)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            FileStream outputStream;
            StreamWriter writer;
            var newPath = Path.Combine(path, newFilename);
            outputStream = new FileStream(newPath, FileMode.Create, FileAccess.Write);
            using (writer = new StreamWriter(outputStream))
            {
                writer.Write(content);
            }
            return newPath;
        }
    }
}
