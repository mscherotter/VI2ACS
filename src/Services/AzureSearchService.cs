using log4net;
using Microsoft.Azure.Search;
using System;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Models;

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

        public SearchServiceClient GetClient()
        {
            if (_client == null)
            {
                _client = new SearchServiceClient(_config.Name, new SearchCredentials(_config.AdminKey));
            }
            return _client;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

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

        #endregion
    }
}
