using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using VIToACS.Models;

namespace VIToACS.Parsers
{
    public static class ThumbnailsParser
    {
        public static IEnumerable<Thumbnail> GetThumbnails(JsonDocument doc)
        {

            if (doc == null)
            {
                throw new NullReferenceException();
            }

            var videos = doc.RootElement.GetProperty("videos");
            var summarizedInsights = doc.RootElement.GetProperty("summarizedInsights");
            var thumbnails = new List<Thumbnail>();

            foreach (var video in videos.EnumerateArray())
            {
                var insights = video.GetProperty("insights");

                var allFaces = insights.GetProperty("faces").EnumerateArray();

                foreach (var shot in insights.GetProperty("shots").EnumerateArray())
                {
                    foreach (var keyFrame in shot.GetProperty("keyFrames").EnumerateArray())
                    {
                        var keyFrameId = keyFrame.GetProperty("id").GetInt32();
                        
                        System.Diagnostics.Debug.WriteLine($"Keyframe {keyFrameId}");

                        var instance = keyFrame.GetProperty("instances").EnumerateArray().First();
                        var faces = new List<Face>();
                        foreach (var face in allFaces)
                        {
                            foreach (var appearance in face.GetProperty("instances").EnumerateArray())
                            {
                                var start = TimeSpan.Parse(appearance.GetProperty("start").GetString(), CultureInfo.InvariantCulture);
                                var end = TimeSpan.Parse(appearance.GetProperty("end").GetString(), CultureInfo.InvariantCulture);
                                var isin = Utils.IsIn(instance, start, end);
                                if (isin)
                                {
                                    faces.Add(new Face(instance, face));
                                }
                            }
                        }

                        var labels = new List<string>();
                        foreach (var label in summarizedInsights.GetProperty("labels").EnumerateArray())
                        {
                            foreach (var labelInstance in label.GetProperty("appearances").EnumerateArray())
                            {
                                var start = TimeSpan.Parse(labelInstance.GetProperty("startTime").GetString(), CultureInfo.InvariantCulture);
                                var end = TimeSpan.Parse(labelInstance.GetProperty("endTime").GetString(), CultureInfo.InvariantCulture);
                                var isin = Utils.IsIn(instance, start, end);
                                if (isin)
                                {
                                    labels.Add(label.GetProperty("name").GetString());
                                }
                            }
                        }

                        var ocr = Utils.CreateCollection(insights, instance, "ocr", delegate (JsonElement item)
                        {
                            return new Ocr
                            {
                                Text = item.GetProperty("text").GetString(),
                                Confidence = item.GetProperty("confidence").GetDouble(),
                                Language = item.GetProperty("language").GetString()
                            };
                        });

                        var keywords = Utils.CreateCollection(insights, instance, "keywords", delegate (JsonElement item)
                        {
                            return Keyword.Create(item);
                        });

                        var topics = Utils.CreateCollection(insights, instance, "topics", delegate (JsonElement item)
                        {
                            return new Topic
                            {
                                Name = item.GetProperty("name").GetString(),
                                Confidence = item.GetProperty("confidence").GetDouble()
                            };
                        });

                        var sentiments = Utils.CreateCollection(insights, instance, "sentiments", delegate (JsonElement sentiment)
                        {
                            return new Sentiment
                            {
                                SentimentType = sentiment.GetProperty("sentimentType").GetString(),
                                AverageScore = sentiment.GetProperty("averageScore").GetDouble(),
                                Start = Utils.GetTimeSpan(instance, "start"),
                                End = Utils.GetTimeSpan(instance, "end")
                            };
                        });

                        var namedLocations = Utils.CreateCollection(insights, instance, "namedLocations", delegate (JsonElement namedLocation)
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

                        string storageAccountName = "";
                        string container = "";
                        var thumbnailId = instance.GetProperty("thumbnailId").GetString();

                        thumbnails.Add(new Thumbnail
                        {
                            Id = thumbnailId,
                            //Uri will be added after this step,
                            Video = Utils.CreateVideo(video),
                            Start = Utils.GetTimeSpan(instance, "start"),
                            End = Utils.GetTimeSpan(instance, "end"),
                            Faces = faces,
                            Labels = labels,
                            Ocr = ocr?.ToList(),
                            Keywords = keywords?.ToList(),
                            Topics = topics?.ToList(),
                            ShotTags = Utils.GetTags(shot),
                            Playlist = Utils.CreatePlaylist(doc.RootElement),
                            Sentiments = sentiments?.ToList(),
                            NamedLocations = namedLocations?.ToList()
                        });
                    }
                }
                
            }

            return thumbnails;
        }
    }
}
