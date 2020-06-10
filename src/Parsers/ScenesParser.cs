using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using VIToACS.Models;

namespace VIToACS.Parsers
{
    public static class ScenesParser
    {
        public static IEnumerable<Scene> GetScenes(JsonDocument doc)
        {
            if (doc == null)
            {
                throw new NullReferenceException();
            }

            var videos = doc.RootElement.GetProperty("videos");
            var video = videos.EnumerateArray().FirstOrDefault();
            var videoId = video.GetProperty("id").GetString();
            var insights = video.GetProperty("insights");
            var scenes = new List<Scene>();

            foreach (var scene in insights.GetProperty("scenes").EnumerateArray())
            {
                var sceneId = scene.GetProperty("id").GetUInt32();
                var instance = scene.GetProperty("instances").EnumerateArray().First();
                var start = TimeSpan.Parse(instance.GetProperty("start").GetString(), CultureInfo.InvariantCulture);
                var end = TimeSpan.Parse(instance.GetProperty("end").GetString(), CultureInfo.InvariantCulture);
                var shots = new List<Shot>();
                foreach (var shot in insights.GetProperty("shots").EnumerateArray())
                {
                    var shotInstance = shot.GetProperty("instances").EnumerateArray().First();
                    var keyframes = new List<KeyFrame>();
                    foreach (var keyFrame in shot.GetProperty("keyFrames").EnumerateArray())
                    {
                        var keyFrameInstance = keyFrame.GetProperty("instances").EnumerateArray().First();
                        var keyFrameStart = TimeSpan.Parse(keyFrameInstance.GetProperty("start").GetString(), CultureInfo.InvariantCulture);
                        var keyFrameEnd = TimeSpan.Parse(keyFrameInstance.GetProperty("end").GetString(), CultureInfo.InvariantCulture);
                        keyframes.Add(new KeyFrame
                        {
                            Id = keyFrame.GetProperty("id").GetInt32(),
                            ThumbnailId = keyFrameInstance.GetProperty("thumbnailId").GetString(),
                            Start = keyFrameStart.TotalSeconds,
                            End = keyFrameEnd.TotalSeconds
                        });

                    }

                    var isin = Utils.IsIn(shotInstance, start, end);
                    var hasTags = shot.TryGetProperty("tags", out JsonElement tags) && isin;
                    var tagList = new List<string>();

                    if (hasTags)
                    {
                        foreach (var tag in shot.GetProperty("tags").EnumerateArray())
                        {
                            tagList.Add(tag.GetString());
                        }
                    }
                    shots.Add(new Shot
                    {
                        Id = shot.GetProperty("id").GetInt32(),
                        Start = Utils.GetTimeSpan(shotInstance, "start"),
                        End = Utils.GetTimeSpan(shotInstance, "end"),
                        Tags = hasTags ? tagList : null,
                        KeyFrames = keyframes
                    });
                }

                var transcript = Utils.GetCollection(insights, "transcript", start, end, delegate (JsonElement transcriptInstance, JsonElement item)
                {
                    return new Transcript
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Text = item.GetProperty("text").GetString(),
                        Confidence = item.GetProperty("confidence").GetDouble(),
                        SpeakerId = item.GetProperty("speakerId").GetInt32(),
                        Language = item.GetProperty("language").GetString(),
                        Start = Utils.GetTimeSpan(transcriptInstance, "start"),
                        End = Utils.GetTimeSpan(transcriptInstance, "end")
                    };
                });

                var faces = Utils.GetCollection(insights, "faces", start, end, delegate (JsonElement faceInstance, JsonElement face)
                {
                    return new Face(faceInstance, face);
                });

                var emotions = Utils.GetCollection(insights, "emotions", start, end, delegate (JsonElement element, JsonElement parent)
                {
                    return new Emotion
                    {
                        Id = parent.GetProperty("id").GetInt32(),
                        Type = parent.GetProperty("type").GetString(),
                        Start = Utils.GetTimeSpan(element, "start"),
                        End = Utils.GetTimeSpan(element, "end")
                    };
                });

                var labels = Utils.GetCollection(insights, "labels", start, end, delegate (JsonElement element, JsonElement parent)
                {
                    string referenceId = null;

                    if (parent.TryGetProperty("referenceId", out JsonElement value))
                    {
                        referenceId = value.GetString();
                    }

                    return new Label
                    {
                        Id = parent.GetProperty("id").GetInt32(),
                        Name = parent.GetProperty("name").GetString(),
                        Start = Utils.GetTimeSpan(element, "start"),
                        End = Utils.GetTimeSpan(element, "end"),
                        Confidence = element.GetProperty("confidence").GetDouble(),
                        ReferenceId = referenceId,
                        Language = parent.GetProperty("language").GetString()
                    };
                });

                var audioEffects = Utils.GetCollection(insights, "audioEffects", start, end, delegate (JsonElement instance, JsonElement effect)
                {
                    return new AudioEffect
                    {
                        Type = effect.GetProperty("type").GetString(),
                        Start = Utils.GetTimeSpan(instance, "start"),
                        End = Utils.GetTimeSpan(instance, "end")
                    };
                });

                var sentiments = Utils.GetCollection(insights, "sentiments", start, end, delegate (JsonElement instance, JsonElement sentiment)
                {
                    return new Sentiment
                    {
                        SentimentType = sentiment.GetProperty("sentimentType").GetString(),
                        AverageScore = sentiment.GetProperty("averageScore").GetDouble(),
                        Start = Utils.GetTimeSpan(instance, "start"),
                        End = Utils.GetTimeSpan(instance, "end")
                    };
                });

                var keywords = Utils.GetCollection(insights, "keywords", start, end, delegate (JsonElement instance, JsonElement keyword)
                {
                    return Keyword.Create(keyword);
                });

                var namedLocations = Utils.GetCollection(insights, "namedLocations", start, end, delegate (JsonElement instance, JsonElement namedLocation)
                {
                    return new NamedLocation
                    {
                        Name = namedLocation.GetProperty("name").GetString(),
                        ReferenceId = namedLocation.GetProperty("referenceId").GetString(),
                        ReferenceUrl = namedLocation.GetProperty("referenceUrl").GetString(),
                        Description = namedLocation.GetProperty("description").GetString(),
                        Confidence = namedLocation.GetProperty("confidence").GetDouble(),
                        IsCustom = namedLocation.GetProperty("isCustom").GetBoolean(),
                    };
                });

                var contentModeration = Utils.GetCollection(insights, "visualContentModeration", start, end, delegate (JsonElement instance, JsonElement c)
                { 
                    return new VisualContentModeration {
                        Id = c.GetProperty("id").GetInt32(),
                        AdultScore = c.GetProperty("adultScore").GetDouble(),
                        RacyScore = c.GetProperty("racyScore").GetDouble()
                    };
                });

                scenes.Add(new Scene
                {
                    Id = $"{videoId}_{sceneId}",
                    Start = start.TotalSeconds,
                    End = end.TotalSeconds,
                    Shots = shots?.ToList(),
                    Video = Utils.CreateVideo(video),
                    Transcript = transcript?.ToList(),
                    Faces = faces?.ToList(),
                    Emotions = emotions?.ToList(),
                    Labels = labels?.ToList(),
                    AudioEffects = audioEffects?.ToList(),
                    Sentiments = sentiments?.ToList(),
                    Playlist = Utils.CreatePlaylist(doc.RootElement),
                    Keywords = keywords?.ToList(),
                    NamedLocations = namedLocations?.ToList(),
                    VisualContentModerations = contentModeration?.ToList()
                });


            }
            return scenes;
        }
    }
}
