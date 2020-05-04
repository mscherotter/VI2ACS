using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace VIToACS
{
    class Program
    {
        private const string Videos = "videos";

        #region Methods
        /// <summary>
        /// Convert Video Indexer JSON output into Azure Cognitive Search input scene documents
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            Console.WriteLine(Resources.Title);

            if (!args.Any())
            {
                Console.WriteLine(Resources.Syntax);

                return;
            }

            var filename = args[0];

            if (!File.Exists(filename))
            {
                Console.WriteLine(Resources.FileNotFoundFormat, args[0]);

                return;
            }

            await CreateSceneIndexAsync(filename).ConfigureAwait(false);

            await CreateThumbnailIndexAxync(filename).ConfigureAwait(false);
        }
        #endregion

        #region Implementation
        private static async Task CreateThumbnailIndexAxync(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open))
            {
                var doc = await JsonDocument.ParseAsync(fileStream).ConfigureAwait(false);

                var videos = doc.RootElement.GetProperty(Videos);

                var summarizedInsights = doc.RootElement.GetProperty("summarizedInsights");

                var allFaces = summarizedInsights.GetProperty("faces").EnumerateArray();

                var thumbnails = from video in videos.EnumerateArray()
                                 let insights = video.GetProperty("insights")
                                 from shot in insights.GetProperty("shots").EnumerateArray()
                                 from keyFrame in shot.GetProperty("keyFrames").EnumerateArray()
                                 let instance = keyFrame.GetProperty("instances").EnumerateArray().First()
                                 let faces = from face in allFaces
                                             from appearance in face.GetProperty("appearances").EnumerateArray()
                                             let start = TimeSpan.Parse(appearance.GetProperty("startTime").GetString(), CultureInfo.InvariantCulture)
                                             let end = TimeSpan.Parse(appearance.GetProperty("endTime").GetString(), CultureInfo.InvariantCulture)
                                             where IsIn(instance, start, end)
                                             select face.GetProperty("name").GetString()
                                 let labels = from label in summarizedInsights.GetProperty("labels").EnumerateArray()
                                              from labelInstance in label.GetProperty("appearances").EnumerateArray()
                                              let start = TimeSpan.Parse(labelInstance.GetProperty("startTime").GetString(), CultureInfo.InvariantCulture)
                                              let end = TimeSpan.Parse(labelInstance.GetProperty("endTime").GetString(), CultureInfo.InvariantCulture)
                                              where IsIn(instance, start, end)
                                              select label.GetProperty("name").GetString()
                                 let ocr = CreateCollection(insights, instance, "ocr", delegate (JsonElement item)
                                 {
                                     return new
                                     {
                                         text = item.GetProperty("text").GetString(),
                                         confidence = item.GetProperty("confidence").GetDouble()
                                     };
                                 })
                                 let keywords = CreateCollection(insights, instance, "keywords", delegate (JsonElement item)
                                 {
                                     return new
                                     {
                                         text = item.GetProperty("text").GetString(),
                                         confidence = item.GetProperty("confidence").GetDouble()
                                     };
                                 })
                                 let topics = CreateCollection(insights, instance, "topics", delegate (JsonElement item)
                                 {
                                     return new
                                     {
                                         name = item.GetProperty("name").GetString(),
                                         confidence = item.GetProperty("confidence").GetDouble()
                                     };
                                 })
                                 select new // this is a document for Azure Cognitive Search
                                 {
                                     id = instance.GetProperty("thumbnailId").GetString(),
                                     //duration = TimeSpan.Parse(insights.GetProperty("duration").GetString(), CultureInfo.InvariantCulture),
                                     //language = insights.GetProperty("language").GetString(),
                                     video = CreateVideo(video),
                                     start = GetTimeSpan(instance, "start"),
                                     end = GetTimeSpan(instance, "end"),
                                     faces = faces.Any() ? faces : null,
                                     labels = labels.Any() ? labels : null,
                                     ocr,
                                     keywords,
                                     topics,
                                     shotTags = GetTags(shot),
                                     playlist = CreatePlaylist(doc.RootElement)
                                 };

                WriteFile("thumbnails_", filename, thumbnails);
            }
        }
        static object GetTags(JsonElement shot)
        {
            if (shot.TryGetProperty("tags", out JsonElement value))
            {
                return from item in value.EnumerateArray()
                       select item.GetString();
            }

            return null;
        }
        static object CreateCollection(JsonElement insights, JsonElement instance, string propertyName, Func<JsonElement, object> itemFunction)
        {
            if (insights.TryGetProperty(propertyName, out JsonElement property))
            {
                //Console.WriteLine("Getting collection " + propertyName);

                var ocr = from item in property.EnumerateArray()
                          from itemInstance in item.GetProperty("instances").EnumerateArray()
                          let start = TimeSpan.Parse(itemInstance.GetProperty("start").GetString(), CultureInfo.InvariantCulture)
                          let end = TimeSpan.Parse(itemInstance.GetProperty("end").GetString(), CultureInfo.InvariantCulture)
                          where IsIn(instance, start, end)
                          select itemFunction(item);

                return ocr.Any() ? ocr : null;
            }

            return null;
        }

        static void WriteFile(string prefix, string filename, object value)
        {
            var newValue = new
            {
                value
            };

            var json = JsonSerializer.Serialize(newValue, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

            var newFilename = prefix + Path.GetFileName(filename);
            var path = Path.GetDirectoryName(filename);
            var newPath = Path.Combine(path, newFilename);
            using (var outputStream = new FileStream(newPath, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(outputStream))
                {
                    writer.Write(json);
                }
            }

            //Console.WriteLine(json);
        }

        private static async Task CreateSceneIndexAsync(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open))
            {
                var doc = await System.Text.Json.JsonDocument.ParseAsync(fileStream).ConfigureAwait(false);
                var videos = doc.RootElement.GetProperty("videos");

                var sceneObjects = from video in videos.EnumerateArray()
                                   let videoId = video.GetProperty("id").GetString()
                                   let insights = video.GetProperty("insights")
                                   from scene in insights.GetProperty("scenes").EnumerateArray()
                                   let sceneId = scene.GetProperty("id").GetUInt32()
                                   let instance = scene.GetProperty("instances").EnumerateArray().First()
                                   let start = TimeSpan.Parse(instance.GetProperty("start").GetString(), CultureInfo.InvariantCulture)
                                   let end = TimeSpan.Parse(instance.GetProperty("end").GetString(), CultureInfo.InvariantCulture)
                                   let shots = from shot in insights.GetProperty("shots").EnumerateArray()
                                               let shotInstance = shot.GetProperty("instances").EnumerateArray().First()
                                               let keyFrames = from keyFrame in shot.GetProperty("keyFrames").EnumerateArray()
                                                               let keyFrameInstance = keyFrame.GetProperty("instances").EnumerateArray().First()
                                                               let keyFrameStart = TimeSpan.Parse(keyFrameInstance.GetProperty("start").GetString(), CultureInfo.InvariantCulture)
                                                               let keyFrameEnd = TimeSpan.Parse(keyFrameInstance.GetProperty("end").GetString(), CultureInfo.InvariantCulture)
                                                               select new
                                                               {
                                                                   id = keyFrame.GetProperty("id").GetUInt32(),
                                                                   thumbnailId = keyFrameInstance.GetProperty("thumbnailId").GetString(),
                                                                   start = keyFrameStart.TotalSeconds,
                                                                   end = keyFrameEnd.TotalSeconds
                                                               }
                                               let hasTags = shot.TryGetProperty("tags", out JsonElement tags)
                                               where IsIn(shotInstance, start, end)
                                               select new
                                               {
                                                   id = shot.GetProperty("id").GetUInt32(),
                                                   start = GetTimeSpan(shotInstance, "start"),
                                                   end = GetTimeSpan(shotInstance, "end"),
                                                   tags = hasTags ? from tag in shot.GetProperty("tags").EnumerateArray()
                                                                    select tag.GetString() : null,
                                                   keyFrames
                                               }
                                   let transcript = GetCollection(insights, "transcript", start, end, delegate (JsonElement transcriptInstance, JsonElement item)
                                   {
                                       return new
                                       {
                                           id = item.GetProperty("id").GetUInt32(),
                                           text = item.GetProperty("text").GetString(),
                                           language = item.GetProperty("language").GetString(),
                                           start = GetTimeSpan(transcriptInstance, "start"),
                                           end = GetTimeSpan(transcriptInstance, "end")
                                       };
                                   })
                                   let faces = GetCollection(insights, "faces", start, end, delegate (JsonElement faceInstance, JsonElement face)
                                   {
                                       return new
                                       {
                                           id = face.GetProperty("id").GetUInt32(),
                                           name = face.GetProperty("name").GetString(),
                                           start = GetTimeSpan(faceInstance, "start"),
                                           end = GetTimeSpan(faceInstance, "end"),
                                           confidence = face.GetProperty("confidence").GetDouble(),
                                           knownPersonId = face.TryGetProperty("knownPersonId", out JsonElement value2) ? face.GetProperty("knownPersonId").GetString() : null
                                       };
                                   })
                                   let emotions = GetCollection(insights, "emotions", start, end, delegate (JsonElement element, JsonElement parent)
                                   {
                                       return new
                                       {
                                           id = parent.GetProperty("id").GetUInt32(),
                                           type = parent.GetProperty("type").GetString(),
                                           start = GetTimeSpan(element, "start"),
                                           end = GetTimeSpan(element, "end")
                                       };
                                   })
                                   let labels = GetCollection(insights, "labels", start, end, delegate (JsonElement element, JsonElement parent)
                                   {
                                       return new
                                       {
                                           id = parent.GetProperty("id").GetUInt32(),
                                           name = parent.GetProperty("name").GetString(),
                                           start = GetTimeSpan(element, "start"),
                                           end = GetTimeSpan(element, "end"),
                                           confidence = element.GetProperty("confidence").GetDouble()
                                       };
                                   })
                                   let audioEffects = GetCollection(insights, "audioEffects", start, end, delegate (JsonElement instance, JsonElement effect)
                                   {
                                       return new
                                       {
                                           type = effect.GetProperty("type").GetString(),
                                           start = GetTimeSpan(instance, "start"),
                                           end = GetTimeSpan(instance, "end")
                                       };
                                   })
                                   let sentiments = GetCollection(insights, "sentiments", start, end, delegate(JsonElement instance, JsonElement sentiment)
                                   {
                                       return new
                                       {
                                           sentimentType = sentiment.GetProperty("sentimentType").GetString(),
                                           averageScore = sentiment.GetProperty("averageScore").GetDouble(),
                                           start = GetTimeSpan(instance, "start"),
                                           end = GetTimeSpan(instance, "end")
                                       };
                                   })
                                   //let textualContentModeration = GetCollection(insights, "textualContentModeration", start, end, delegate(JsonElement instance, JsonElement  )
                                   select new // this is a document going into azure cognitive search 
                                   {
                                       id = $"{videoId}_{sceneId}",
                                       start = start.TotalSeconds,
                                       end = end.TotalSeconds,
                                       shots,
                                       video = CreateVideo(video),
                                       transcript,
                                       faces,
                                       emotions,
                                       labels,
                                       audioEffects,
                                       sentiments,
                                       playlist = CreatePlaylist(doc.RootElement)
                                   };
                WriteFile("scenes_", filename, sceneObjects);
            }
        }

        private static object CreatePlaylist(JsonElement rootElement)
        {
            return new
            {
                name = rootElement.GetProperty("name").GetString(),
                description = rootElement.GetProperty("description").GetString(),
                privacyMode = rootElement.GetProperty("privacyMode").GetString(),
                state = rootElement.GetProperty("state").GetString(),
                accountId = rootElement.GetProperty("accountId").GetString(),
                id = rootElement.GetProperty("id").GetString(),
                userName = rootElement.GetProperty("userName").GetString(),
                created = rootElement.GetProperty("created").GetDateTimeOffset(),
                isOwned = rootElement.GetProperty("isOwned").GetBoolean(),
                isEditable = rootElement.GetProperty("isEditable").GetBoolean(),
                isBase= rootElement.GetProperty("isBase").GetBoolean(),
                durationInSeconds = rootElement.GetProperty("durationInSeconds").GetDouble()
            };
        }

        static object CreateVideo(JsonElement video)
        {
            string thumbnailId = null;
            if (video.TryGetProperty("thumbnailId", out JsonElement jsonElement))
            {
                thumbnailId = jsonElement.GetString();
            }

            return new
            {
                id = video.GetProperty("id").GetString(),
                thumbnailId,
                detectSourceLanguage = video.GetProperty("detectSourceLanguage").GetBoolean(),
                sourceLanguages = from item in video.GetProperty("sourceLanguages").EnumerateArray()
                                select item.GetString()
            };
        }

        static bool IsIn(JsonElement element, TimeSpan start, TimeSpan end)
        {
            var elementStart = TimeSpan.Parse(element.GetProperty("start").GetString(), CultureInfo.InvariantCulture);
            var elementEnd = TimeSpan.Parse(element.GetProperty("end").GetString(), CultureInfo.InvariantCulture);
            return elementStart >= start && elementEnd <= end;


        }

        static double GetTimeSpan(JsonElement element, string name)
        {
            if (TimeSpan.TryParse(element.GetProperty(name).GetString(), out TimeSpan result))
            {
                return result.TotalSeconds;
            }

            return 0;
        }

        static IEnumerable GetCollection(JsonElement element, string name, TimeSpan start, TimeSpan end, Func<JsonElement, JsonElement, object> func)
        {
            if (element.TryGetProperty(name, out JsonElement property))
            {
                var emotions = from emotion in property.EnumerateArray()
                               from emotionInstance in emotion.GetProperty("instances").EnumerateArray()
                               where IsIn(emotionInstance, start, end)
                               select func(emotionInstance, emotion);
                if (emotions.Any())
                {
                    return emotions;
                }
            }

            return null;
        }
        #endregion
    }
}
