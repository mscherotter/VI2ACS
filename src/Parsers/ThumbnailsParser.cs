using System;
using System.Collections.Generic;
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
            var allFaces = summarizedInsights.GetProperty("faces").EnumerateArray();
            var thumbnails = new List<Thumbnail>();

            foreach (var video in videos.EnumerateArray())
            {
                var insights = video.GetProperty("insights");
                foreach (var shot in insights.GetProperty("shots").EnumerateArray())
                {
                    foreach (var keyFrame in shot.GetProperty("keyFrames").EnumerateArray())
                    {
                        var instance = keyFrame.GetProperty("instances").EnumerateArray().First();
                        var faces = new List<Face>();
                        foreach (var face in allFaces)
                        {
                            foreach (var appearance in face.GetProperty("appearances").EnumerateArray())
                            {
                                var start = TimeSpan.Parse(appearance.GetProperty("startTime").GetString(), CultureInfo.InvariantCulture);
                                var end = TimeSpan.Parse(appearance.GetProperty("endTime").GetString(), CultureInfo.InvariantCulture);
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
                                Confidence = item.GetProperty("confidence").GetDouble()
                            };
                        });

                        var keywords = Utils.CreateCollection(insights, instance, "keywords", delegate (JsonElement item)
                        {
                            return new Keyword
                            {
                                Text = item.GetProperty("text").GetString(),
                                Confidence = item.GetProperty("confidence").GetDouble()
                            };
                        });

                        var topics = Utils.CreateCollection(insights, instance, "topics", delegate (JsonElement item)
                        {
                            return new Topic
                            {
                                Name = item.GetProperty("name").GetString(),
                                Confidence = item.GetProperty("confidence").GetDouble()
                            };
                        });

                        thumbnails.Add(new Thumbnail
                        {
                            Id = instance.GetProperty("thumbnailId").GetString(),
                            Video = Utils.CreateVideo(video),
                            Start = Utils.GetTimeSpan(instance, "start"),
                            End = Utils.GetTimeSpan(instance, "end"),
                            Faces = faces.Any() ? faces : null,
                            Labels = labels.Any() ? labels : null,
                            Ocr = (ocr != null) ? ocr.ToList() : null,
                            Keywords = (keywords != null) ? keywords.ToList() : null,
                            Topics = (topics != null) ? topics.ToList() : null,
                            ShotTags = Utils.GetTags(shot),
                            Playlist = Utils.CreatePlaylist(doc.RootElement)
                        });
                    }
                }
                
            }

            return thumbnails;
        }
    }
}
