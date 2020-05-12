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
    public class FileStreamInsightsReaderService : IInsightsReader
    {
        private readonly ReaderConfig _config;
        private readonly ILog _logger;


        public FileStreamInsightsReaderService(ReaderConfig config, ILog logger)
        {
            if (config == null || logger == null)
                throw new NullReferenceException();
            _config = config;
            _logger = logger;
        }

        public IEnumerable<ParsedDocument> ReadInsightsFiles()
        {
            foreach (string file in Directory.EnumerateFiles(_config.FileStream.InsightsPath, "*.json"))
            {
                string scenesJson = string.Empty;
                string thumbnailsJson = string.Empty;

                _logger.Debug($"Reading the file { file }.");

                IEnumerable<Scene> scenes = GetScenes(file);
                if (scenes == null)
                {
                    _logger.Warn($"It was not possible to extract the scenes from the file { file }.");
                }
                else
                {
                    scenesJson = JsonSerializer.Serialize(scenes, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
                    _logger.Debug($"The file { file } has { scenes.Count() } scenes.");
                }

                IEnumerable<Thumbnail> thumbnails = GetThumbnails(file);
                if (thumbnails == null)
                {
                    _logger.Warn($"It was not possible to thumbnails the scenes from the file { file }.");
                }
                else
                {
                    thumbnailsJson = JsonSerializer.Serialize(thumbnails, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
                    _logger.Debug($"The file { file } has { thumbnails.Count() } thumbnails.");
                }

                yield return new ParsedDocument
                {
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
    }
}
