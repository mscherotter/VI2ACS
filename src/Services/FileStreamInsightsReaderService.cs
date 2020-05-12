using log4net;
using System;
using System.Collections.Generic;
using System.IO;
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
            _config = config;
            _logger = logger;
        }

        public IEnumerable<ParsedDocument> ReadInsightsFiles()
        {
            foreach (string file in Directory.EnumerateFiles(_config.FileStream.InsightsPath, "*.json"))
            {
                _logger.Info($"Reading the file { file }.");

                IEnumerable<Scene> scenes = GetScenes(file);
                var scenesJson = JsonSerializer.Serialize(scenes, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

                IEnumerable<Thumbnail> thumbnails = GetThumbnails(file);
                var thumbnailsJson = JsonSerializer.Serialize(thumbnails, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

                yield return new ParsedDocument { FileName = file, ParsedScenesJson = scenesJson, ParsedThumbnailsJson = thumbnailsJson };
            }
        }

        private IEnumerable<Scene> GetScenes(string fileName)
        {
            _logger.Info($"Parsing scenes in the file { fileName }.");
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            return scenes;
        }

        private IEnumerable<Thumbnail> GetThumbnails(string fileName)
        {
            _logger.Info($"Parsing thumbnails in the file { fileName }.");
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            return thumbnails;
        }
    }
}
