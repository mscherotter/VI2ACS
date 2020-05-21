using log4net;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VIToACS.Configurations;
using VIToACS.Interfaces;
using VIToACS.Models;


namespace VIToACS.Services
{
    public class VideoIndexerService : IVideoIndexer
    {
        private readonly VideoIndexerConfig _config;
        private readonly ReaderConfig _readerConfig;
        private readonly ILog _logger;
        private static string _apiUrl = "https://api.videoindexer.ai";
        private const bool _allowEdit = false;

        // Cached access token
        private string _accountAccessToken;

        // Timeout for cached access token
        private DateTime _accountAccessTokenTimeStamp;

        public VideoIndexerService(VideoIndexerConfig config, ReaderConfig readerConfig, ILog logger)
        {
            if (config == null || readerConfig == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _readerConfig = readerConfig;
            _logger = logger;
        }

        public async Task AddNewInsightsFileToReaderAsync(IInsightsReader reader, MediaAsset media)
        {
            var accessToken = await GetAccountAccessTokenAsync();

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var client = GetHttpClient();

                    var apiUrl = $"{_apiUrl}/{_config.Location}/Accounts/{_config.AccountId}/Videos/{media.Id}/Index?accessToken={accessToken}";

                    var result = client.GetAsync(apiUrl).Result;
                    var resultJson = result.Content.ReadAsStringAsync().Result;

                    reader.AddNewFile(GetNewMediaFileName(media), resultJson);
                }
                catch (HttpRequestException ex)
                {
                    _logger.Error($"Error reading the videos for the account { _config.AccountId }. Message: { ex.Message }");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                }
            }
            else
            {
                _logger.Error($"Invalid access token for the account { _config.AccountId }.");
            }
        }

        public async Task<MediaAssetResults> ListVideosAsync(int skip)
        {
            // Get an access token and create the client
            var accessToken = await GetAccountAccessTokenAsync();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var client = GetHttpClient();

                    // Get the results in JSON format
                    var listVideosRequestResult = client.GetAsync($"{_apiUrl}/{_config.Location}/Accounts/{_config.AccountId}/Videos?pageSize={_config.PageSize}&skip={skip}&accessToken={accessToken}").Result;
                    var listVideostJson = listVideosRequestResult.Content.ReadAsStringAsync().Result;

                    // Parse and return the results
                    var results = JsonConvert.DeserializeObject<MediaAssetResults>(listVideostJson);
                    return results;
                }
                catch (HttpRequestException ex)
                {
                    _logger.Error($"Error reading the videos for the account { _config.AccountId }. Message: { ex.Message }");
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
            else
            {
                _logger.Error($"Invalid access token for the account { _config.AccountId }.");
                return null;
            }
        }

        private async Task<string> GetAccountAccessTokenAsync()
        {
            // Check to see if we can reuse the cached access token
            if ((DateTime.UtcNow - _accountAccessTokenTimeStamp).TotalMinutes > 55)
            {
                try
                {
                    var client = GetHttpClient();

                    // Add the API key to the default request headers
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);

                    // Call the API to get the access token
                    var accountAccessTokenRequestResult = await
                        client.GetAsync($"{ _apiUrl }/auth/{ _config.Location }/Accounts/{ _config.AccountId }/AccessToken?allowEdit={ _allowEdit }");

                    // Parse the access token
                    _accountAccessToken = accountAccessTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");
                    _accountAccessTokenTimeStamp = DateTime.UtcNow;
                }
                catch(HttpRequestException ex)
                {
                    _logger.Error($"Error getting the access token for the account { _config.AccountId }. Message: { ex.Message }");
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return string.Empty;
                }
            }
            return _accountAccessToken;
        }

        private HttpClient GetHttpClient()
        {
            // Create a new HTTP Client
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);
            return client;
        }

        private string GetNewMediaFileName(MediaAsset media)
        {
            int maxSize = 120;
            var newName = Path.GetInvalidFileNameChars().Aggregate(media.Name, (current, c) => current.Replace(c.ToString(), string.Empty));
            newName = newName
                .Replace(" ", "_")
                .Replace("/", "_")
                .Replace(".", "_")
                .Replace("%", "_")
                .ToLower();

            return $"{ newName.Substring(1, ( newName.Length > maxSize ? maxSize : newName.Length - 1)) }_{ media.Id.ToLower() }.json";
        }
    }
}
