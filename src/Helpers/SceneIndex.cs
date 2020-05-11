using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace VIToACS.Helpers
{
    public static class SceneIndex
    {
        private static async Task CreateSceneIndexAsync(string filename)
        {
            using var fileStream = new FileStream(filename, FileMode.Open);

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
                                           where Utils.IsIn(shotInstance, start, end)
                                           select new
                                           {
                                               id = shot.GetProperty("id").GetUInt32(),
                                               start = Utils.GetTimeSpan(shotInstance, "start"),
                                               end = Utils.GetTimeSpan(shotInstance, "end"),
                                               tags = hasTags ? from tag in shot.GetProperty("tags").EnumerateArray()
                                                                select tag.GetString() : null,
                                               keyFrames
                                           }
                               let transcript = Utils.GetCollection(insights, "transcript", start, end, delegate (JsonElement transcriptInstance, JsonElement item)
                               {
                                   return new
                                   {
                                       id = item.GetProperty("id").GetUInt32(),
                                       text = item.GetProperty("text").GetString(),
                                       language = item.GetProperty("language").GetString(),
                                       start = Utils.GetTimeSpan(transcriptInstance, "start"),
                                       end = Utils.GetTimeSpan(transcriptInstance, "end")
                                   };
                               })
                               let faces = Utils.GetCollection(insights, "faces", start, end, delegate (JsonElement faceInstance, JsonElement face)
                               {
                                   return new
                                   {
                                       id = face.GetProperty("id").GetUInt32(),
                                       name = face.GetProperty("name").GetString(),
                                       start = Utils.GetTimeSpan(faceInstance, "start"),
                                       end = Utils.GetTimeSpan(faceInstance, "end"),
                                       confidence = face.GetProperty("confidence").GetDouble(),
                                       knownPersonId = face.TryGetProperty("knownPersonId", out JsonElement value2) ? face.GetProperty("knownPersonId").GetString() : null
                                   };
                               })
                               let emotions = Utils.GetCollection(insights, "emotions", start, end, delegate (JsonElement element, JsonElement parent)
                               {
                                   return new
                                   {
                                       id = parent.GetProperty("id").GetUInt32(),
                                       type = parent.GetProperty("type").GetString(),
                                       start = Utils.GetTimeSpan(element, "start"),
                                       end = Utils.GetTimeSpan(element, "end")
                                   };
                               })
                               let labels = Utils.GetCollection(insights, "labels", start, end, delegate (JsonElement element, JsonElement parent)
                               {
                                   return new
                                   {
                                       id = parent.GetProperty("id").GetUInt32(),
                                       name = parent.GetProperty("name").GetString(),
                                       start = Utils.GetTimeSpan(element, "start"),
                                       end = Utils.GetTimeSpan(element, "end"),
                                       confidence = element.GetProperty("confidence").GetDouble()
                                   };
                               })
                               let audioEffects = Utils.GetCollection(insights, "audioEffects", start, end, delegate (JsonElement instance, JsonElement effect)
                               {
                                   return new
                                   {
                                       type = effect.GetProperty("type").GetString(),
                                       start = Utils.GetTimeSpan(instance, "start"),
                                       end = Utils.GetTimeSpan(instance, "end")
                                   };
                               })
                               let sentiments = Utils.GetCollection(insights, "sentiments", start, end, delegate (JsonElement instance, JsonElement sentiment)
                               {
                                   return new
                                   {
                                       sentimentType = sentiment.GetProperty("sentimentType").GetString(),
                                       averageScore = sentiment.GetProperty("averageScore").GetDouble(),
                                       start = Utils.GetTimeSpan(instance, "start"),
                                       end = Utils.GetTimeSpan(instance, "end")
                                   };
                               })
                               //let textualContentModeration = GetCollection(insights, "textualContentModeration", start, end, delegate(JsonElement instance, JsonElement  )
                               select new // this is a document going into azure cognitive search 
                               {
                                   id = $"{videoId}_{sceneId}",
                                   start = start.TotalSeconds,
                                   end = end.TotalSeconds,
                                   shots,
                                   video = Utils.CreateVideo(video),
                                   transcript,
                                   faces,
                                   emotions,
                                   labels,
                                   audioEffects,
                                   sentiments,
                                   playlist = Utils.CreatePlaylist(doc.RootElement)
                               };
            Utils.WriteFile("scenes_", filename, sceneObjects);
        }
    }
}
