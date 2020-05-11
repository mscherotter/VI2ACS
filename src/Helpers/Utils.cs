using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace VIToACS.Helpers
{
    public static class Utils
    {
        public static object GetTags(JsonElement shot)
        {
            if (shot.TryGetProperty("tags", out JsonElement value))
            {
                return from item in value.EnumerateArray()
                       select item.GetString();
            }

            return null;
        }

        public static object CreateCollection(JsonElement insights, JsonElement instance, string propertyName, Func<JsonElement, object> itemFunction)
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

        public static void WriteFile(string prefix, string filename, object value)
        {
            var newValue = new
            {
                value
            };

            var json = JsonSerializer.Serialize(newValue, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

            var newFilename = prefix + Path.GetFileName(filename);
            var path = Path.GetDirectoryName(filename);
            var newPath = Path.Combine(path, newFilename);

            using var outputStream = new FileStream(newPath, FileMode.Create, FileAccess.Write);

            using var writer = new StreamWriter(outputStream);

            writer.Write(json);

            //Console.WriteLine(json);
        }


        public static object CreatePlaylist(JsonElement rootElement)
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
                isBase = rootElement.GetProperty("isBase").GetBoolean(),
                durationInSeconds = rootElement.GetProperty("durationInSeconds").GetDouble()
            };
        }

        public static object CreateVideo(JsonElement video)
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

        public static bool IsIn(JsonElement element, TimeSpan start, TimeSpan end)
        {
            var elementStart = TimeSpan.Parse(element.GetProperty("start").GetString(), CultureInfo.InvariantCulture);
            var elementEnd = TimeSpan.Parse(element.GetProperty("end").GetString(), CultureInfo.InvariantCulture);
            return elementStart >= start && elementEnd <= end;


        }

        public static double GetTimeSpan(JsonElement element, string name)
        {
            if (TimeSpan.TryParse(element.GetProperty(name).GetString(), out TimeSpan result))
            {
                return result.TotalSeconds;
            }

            return 0;
        }

        public static IEnumerable GetCollection(JsonElement element, string name, TimeSpan start, TimeSpan end, Func<JsonElement, JsonElement, object> func)
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
    }
}
