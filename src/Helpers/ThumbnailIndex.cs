using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace VIToACS.Helpers
{
    public static class ThumbnailIndex
    {

        public static async Task CreateThumbnailIndexAxync(string filename)
        {
            using var fileStream = new FileStream(filename, FileMode.Open);

            var doc = await JsonDocument.ParseAsync(fileStream).ConfigureAwait(false);

            var videos = doc.RootElement.GetProperty("videos");

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
                                         where Utils.IsIn(instance, start, end)
                                         select face.GetProperty("name").GetString()
                             let labels = from label in summarizedInsights.GetProperty("labels").EnumerateArray()
                                          from labelInstance in label.GetProperty("appearances").EnumerateArray()
                                          let start = TimeSpan.Parse(labelInstance.GetProperty("startTime").GetString(), CultureInfo.InvariantCulture)
                                          let end = TimeSpan.Parse(labelInstance.GetProperty("endTime").GetString(), CultureInfo.InvariantCulture)
                                          where Utils.IsIn(instance, start, end)
                                          select label.GetProperty("name").GetString()
                             let ocr = Utils.CreateCollection(insights, instance, "ocr", delegate (JsonElement item)
                             {
                                 return new
                                 {
                                     text = item.GetProperty("text").GetString(),
                                     confidence = item.GetProperty("confidence").GetDouble()
                                 };
                             })
                             let keywords = Utils.CreateCollection(insights, instance, "keywords", delegate (JsonElement item)
                             {
                                 return new
                                 {
                                     text = item.GetProperty("text").GetString(),
                                     confidence = item.GetProperty("confidence").GetDouble()
                                 };
                             })
                             let topics = Utils.CreateCollection(insights, instance, "topics", delegate (JsonElement item)
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
                                 video = Utils.CreateVideo(video),
                                 start = Utils.GetTimeSpan(instance, "start"),
                                 end = Utils.GetTimeSpan(instance, "end"),
                                 faces = faces.Any() ? faces : null,
                                 labels = labels.Any() ? labels : null,
                                 ocr,
                                 keywords,
                                 topics,
                                 shotTags = Utils.GetTags(shot),
                                 playlist = Utils.CreatePlaylist(doc.RootElement)
                             };

            Utils.WriteFile("thumbnails_", filename, thumbnails);
        }

    }
}
