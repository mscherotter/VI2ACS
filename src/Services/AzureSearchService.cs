using log4net;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Nest;
using System;
using System.Collections.Generic;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Models;
using IndexAction = Microsoft.Azure.Search.Models.IndexAction;

namespace VIToACS.Services
{
    public class AzureSearchService : IAzureSearch, IDisposable
    {
        private readonly AzureSearchConfig _config;
        private readonly ILog _logger;
        private SearchServiceClient _client;

        public AzureSearchService(AzureSearchConfig config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }
        public SearchServiceClient GetClient()
        {
            if (_client == null)
            {
                _client = new SearchServiceClient(_config.Name, new SearchCredentials(_config.AdminKey));
            }
            return _client;
        }

        public void CreateSceneIndex()
        {
            if (_config.DeleteIndexIfExists)
            {
                DeleteIndexIfExists(_config.SceneIndexName, GetClient());
            }
            var definition = new Microsoft.Azure.Search.Models.Index()
            {
                Name = _config.SceneIndexName,
                Fields = FieldBuilder.BuildForType<Scene>()
            };

            GetClient().Indexes.Create(definition);
        }

        public void CreateThumbnailIndex()
        {
            if (_config.DeleteIndexIfExists)
            {
                DeleteIndexIfExists(_config.ThumbnailIndexName, GetClient());
            }

            var definition = new Microsoft.Azure.Search.Models.Index()
            {
                Name = _config.ThumbnailIndexName,
                Fields = FieldBuilder.BuildForType<Thumbnail>()
            };

            GetClient().Indexes.Create(definition);
        }

        private void DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                _logger.Info($"Deleting the index { indexName }.");
                serviceClient.Indexes.Delete(indexName);
            }
        }


        public void UploadSceneDocuments(IEnumerable<Scene> sceneDocuments)
        {
            SearchIndexClient indexClient = new SearchIndexClient(_config.Name, _config.SceneIndexName, GetClient().SearchCredentials);
            var actions = new List<IndexAction<Scene>>();
            foreach(var sceneDocument in sceneDocuments)
            {
                actions.Add(IndexAction.MergeOrUpload(sceneDocument));
            }
            var batch = IndexBatch.New(actions);
            try
            {
                indexClient.Documents.Index(batch);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public void UploadThumbnailDocuments(IEnumerable<Thumbnail> thumbnailDocuments)
        {
            SearchIndexClient indexClient = new SearchIndexClient(_config.Name, _config.ThumbnailIndexName, GetClient().SearchCredentials);
            var actions = new List<IndexAction<Thumbnail>>();
            foreach (var thumbnailDocument in thumbnailDocuments)
            {
                actions.Add(IndexAction.MergeOrUpload(thumbnailDocument));
            }
            var batch = IndexBatch.New(actions);
            try { 
                indexClient.Documents.Index(batch);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        private bool disposedValue = false;

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
