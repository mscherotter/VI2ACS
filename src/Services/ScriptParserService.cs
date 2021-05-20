﻿using log4net;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Models.Script;

namespace VIToACS.Services
{
    public class ScriptParserService : IAzureSearch, IDisposable
    {
        private readonly AzureSearchConfig _config;
        private readonly ILog _logger;
        private SearchServiceClient _client;
        private bool disposedValue = false;

        public ScriptParserService(AzureSearchConfig config, ILog logger)
        {
            if (config == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _logger = logger;
            _client = new SearchServiceClient(_config.Name, new SearchCredentials(_config.AdminKey));
        }

        public void CreateSceneIndex()
        {
            string indexName = _config.SceneIndexName;
            if (_config.DeleteIndexIfExists)
            {
                DeleteIndexIfExists(indexName, _client);
            }
            if (!_client.Indexes.Exists(indexName))
            {
                var definition = new Microsoft.Azure.Search.Models.Index()
                {
                    Name = indexName,
                    Fields = FieldBuilder.BuildForType<Models.Script.Scene>()
                };
                _client.Indexes.Create(definition);
            }
            else
            {
                _logger.Warn($"The index { indexName } already exists.");
            }
        }

        public void CreateThumbnailIndex()
        {
            //string indexName = _config.ThumbnailIndexName;
            //if (_config.DeleteIndexIfExists)
            //{
            //    DeleteIndexIfExists(indexName, _client);
            //}
            //if (!_client.Indexes.Exists(indexName))
            //{
            //    var definition = new Microsoft.Azure.Search.Models.Index()
            //    {
            //        Name = indexName,
            //        Fields = FieldBuilder.BuildForType<Thumbnail>()
            //    };
            //    _client.Indexes.Create(definition);
            //}
            //else
            //{
            //    _logger.Warn($"The index { indexName } already exists.");
            //}
        }


        private void DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                _logger.Warn($"Deleting the index { indexName }.");
                serviceClient.Indexes.Delete(indexName);
            }
        }

        public void UploadSceneDocuments<T>(IEnumerable<T> sceneDocuments)
        {
            if (sceneDocuments == null)
                throw new NullReferenceException();

            SearchIndexClient indexClient = new SearchIndexClient(_config.Name, _config.SceneIndexName, _client.SearchCredentials);
            var actions = new List<IndexAction<T>>();
            foreach(var sceneDocument in sceneDocuments)
            {
                actions.Add(IndexAction.MergeOrUpload(sceneDocument));
            }
            var batch = IndexBatch.New(actions);
            try
            {
                indexClient.Documents.Index(batch);
            }
            catch(IndexBatchException ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            finally
            {
                indexClient.Dispose();
            }
        }

        public void UploadThumbnailDocuments(IEnumerable<Models.Thumbnail> thumbnailDocuments)
        {
            //if (thumbnailDocuments == null)
            //    throw new NullReferenceException();

            //SearchIndexClient indexClient = new SearchIndexClient(_config.Name, _config.ThumbnailIndexName, _client.SearchCredentials);
            //var actions = new List<IndexAction<Thumbnail>>();
            //foreach (var thumbnailDocument in thumbnailDocuments)
            //{
            //    actions.Add(IndexAction.MergeOrUpload(thumbnailDocument));
            //}
            //var batch = IndexBatch.New(actions);
            //try { 
            //    indexClient.Documents.Index(batch);
            //}
            //catch (IndexBatchException ex)
            //{
            //    _logger.Error(ex.Message);
            //    throw;
            //}
            //finally
            //{
            //    indexClient.Dispose();
            //}
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
