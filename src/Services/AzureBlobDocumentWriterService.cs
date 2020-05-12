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
    public class AzureBlobDocumentWriterService : IDocumentWriter
    {
        private readonly WriterConfig _config;
        private readonly ILog _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _scenesContainerClient;
        private readonly BlobContainerClient _thumbnailsContainerClient;

        public AzureBlobDocumentWriterService(WriterConfig config, ILog logger)
        {
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
            var newPath = WriteFile(_config.AzureBlob.TempUploadFilePath, content, newFilename);

            try
            {
                // Get a reference to a blob
                BlobClient blobClient = _scenesContainerClient.GetBlobClient(newFilename);
                using FileStream uploadFileStream = File.OpenRead(newPath);
                blobClient.Upload(uploadFileStream, true);
                uploadFileStream.Close();
                File.Delete(newPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public void WriteThumbnailsDocument(string fileName, string content)
        {
            var newFilename = _config.ThumbnailsDocumentPrefix + Path.GetFileName(fileName);
            _logger.Info($"Writing the file { newFilename }.");
            var newPath = WriteFile(_config.AzureBlob.TempUploadFilePath, content, newFilename);

            try
            { 
                // Get a reference to a blob
                BlobClient blobClient = _thumbnailsContainerClient.GetBlobClient(newFilename);
                using FileStream uploadFileStream = File.OpenRead(newPath);
                blobClient.Upload(uploadFileStream, true);
                uploadFileStream.Close();
                File.Delete(newPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
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
