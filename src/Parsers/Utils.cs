using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using VIToACS.Models;

namespace VIToACS.Parsers
{
    public static class Utils
    {
        public static List<string> GetTags(JsonElement shot)
        {
            if (shot.TryGetProperty("tags", out JsonElement value))
            {
                var tags = from item in value.EnumerateArray()
                           select item.GetString();

                return tags.ToList();
            }

            return null;
        }

        public static IEnumerable<T> CreateCollection<T>(JsonElement insights, JsonElement instance, string propertyName, Func<JsonElement, T> itemFunction)
        {
            if (insights.TryGetProperty(propertyName, out JsonElement property))
            {

                var items = from item in property.EnumerateArray()
                            from itemInstance in item.GetProperty("instances").EnumerateArray()
                            let start = TimeSpan.Parse(itemInstance.GetProperty("start").GetString(), CultureInfo.InvariantCulture)
                            let end = TimeSpan.Parse(itemInstance.GetProperty("end").GetString(), CultureInfo.InvariantCulture)
                            where IsIn(instance, start, end)
                            select itemFunction(item);

                return items.Any() ? items : null;
            }

            return null;
        }

        public static Playlist CreatePlaylist(JsonElement rootElement)
        {
            return new Playlist
            {
                Name = rootElement.GetProperty("name").GetString(),
                Description = rootElement.GetProperty("description").GetString(),
                PrivacyMode = rootElement.GetProperty("privacyMode").GetString(),
                State = rootElement.GetProperty("state").GetString(),
                AccountId = rootElement.GetProperty("accountId").GetString(),
                Id = rootElement.GetProperty("id").GetString(),
                UserName = rootElement.GetProperty("userName").GetString(),
                Created = rootElement.GetProperty("created").GetDateTimeOffset(),
                IsOwned = rootElement.GetProperty("isOwned").GetBoolean(),
                IsEditable = rootElement.GetProperty("isEditable").GetBoolean(),
                IsBase = rootElement.GetProperty("isBase").GetBoolean(),
                DurationInSeconds = rootElement.GetProperty("durationInSeconds").GetDouble()
            };
        }

        public static Video CreateVideo(JsonElement video)
        {
            string thumbnailId = null;

            if (video.TryGetProperty("thumbnailId", out JsonElement jsonElement))
            {
                thumbnailId = jsonElement.GetString();
            }

            var sourceLanguages = from item in video.GetProperty("sourceLanguages").EnumerateArray()
                                  select item.GetString();

            return new Video
            {
                Id = video.GetProperty("id").GetString(),
                ThumbnailId = thumbnailId,
                DetectSourceLanguage = video.GetProperty("detectSourceLanguage").GetBoolean(),
                SourceLanguages = sourceLanguages.ToList()
            };
        }

        public static bool IsIn(JsonElement element, TimeSpan start, TimeSpan end)
        {
            var elementStart = TimeSpan.Parse(element.GetProperty("start").GetString(), CultureInfo.InvariantCulture);
            var elementEnd = TimeSpan.Parse(element.GetProperty("end").GetString(), CultureInfo.InvariantCulture);

            bool isIn = (elementStart >= start && elementStart <= end) || (elementEnd >= start && elementEnd <= end);

            return isIn;
        }

        public static double GetTimeSpan(JsonElement element, string name)
        {
            var timeSpanText = element.GetProperty(name).GetString();

            if (TimeSpan.TryParse(timeSpanText, out TimeSpan result))
            {
                var totalSeconds = result.TotalSeconds;

                return totalSeconds;
            }

            return 0;
        }

        public static IEnumerable<T> GetCollection<T>(JsonElement element, string name, TimeSpan start, TimeSpan end, Func<JsonElement, JsonElement, T> func)
        {
            if (element.TryGetProperty(name, out JsonElement property))
            {
                var items = from item in property.EnumerateArray()
                            from itemInstance in item.GetProperty("instances").EnumerateArray()
                            where IsIn(itemInstance, start, end)
                            select func(itemInstance, item);
                if (items.Any())
                {
                    return items;
                }
            }

            return null;
        }
    }
}
