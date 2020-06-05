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
        private readonly static string _apiUrl = "https://api.videoindexer.ai";
        private const bool _allowEdit = false;

        // Cached access token
        private static string _accountAccessToken;

        // Timeout for cached access token
        private DateTime _accountAccessTokenTimeStamp;

        public VideoIndexerService(VideoIndexerConfig config, ReaderConfig readerConfig, ILog logger)
        {
            if (config == null || readerConfig == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _readerConfig = readerConfig;
            _logger = logger;

            if (string.IsNullOrEmpty(_config.AccessToken))
            {
                _accountAccessToken = GetAccountAccessTokenAsync().GetAwaiter().GetResult();
                _accountAccessTokenTimeStamp = DateTime.UtcNow;
            }
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

        public bool IsDownloadInsightsEnabled()
        {
            return _config.DownloadInsights;
        }

        public bool IsDownloadThumbnailsEnabled()
        {
            return _config.DownloadThumbnails;
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
                    var listVideosJson = listVideosRequestResult.Content.ReadAsStringAsync().Result;

                    if (listVideosJson.Contains("ACCESS_TOKEN_VALIDATION_FAILED"))
                    {
                        _logger.Error($"Invalid access token!");
                    }

                    // Parse and return the results
                    var results = JsonConvert.DeserializeObject<MediaAssetResults>(listVideosJson);
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

        public async Task DownloadThumbnailAsync(IDocumentWriter writer, string fileName, string videoId, string thumbnailId)
        {
            // Get an access token and create the client
            var accessToken = await GetAccountAccessTokenAsync();

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var client = GetHttpClient();

                    var response = client.GetAsync($"{_apiUrl}/{_config.Location}/Accounts/{_config.AccountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?format=Jpeg&accessToken={accessToken}").Result;

                    if (response.Content.ReadAsStringAsync().Result.Contains("ACCESS_TOKEN_VALIDATION_FAILED"))
                    {
                        _logger.Error($"Invalid access token!");
                    }

                    var bytes = response.Content.ReadAsByteArrayAsync().Result;

                    writer.WriteThumbnailImage(fileName, bytes);

                }
                catch (HttpRequestException ex)
                {
                    _logger.Error($"Error downloading the thumbnail { thumbnailId }. File { fileName }. Message: { ex.Message }");
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

        private async Task<string> GetAccountAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_config.AccessToken))
            {
                return _config.AccessToken;
            }

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

                    if (accountAccessTokenRequestResult.IsSuccessStatusCode)
                    {
                        // Parse the access token
                        _accountAccessToken = accountAccessTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");
                        _accountAccessTokenTimeStamp = DateTime.UtcNow;
                    }
                    else
                    {
                        _logger.Error($"Error using access token for video indexer: {accountAccessTokenRequestResult.ReasonPhrase}");

                        return string.Empty;
                    }
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

            return $"{ newName.Substring(0, ( newName.Length > maxSize ? maxSize : newName.Length - 1)) }_{ media.Id.ToLower() }.json";
        }
    }
}
