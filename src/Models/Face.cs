﻿using Microsoft.Azure.Search;
using Nest;
using System.Text.Json;
using VIToACS.Parsers;

namespace VIToACS.Models
{
    public class Face
    {
        public Face() { }

        /// <summary>
        /// Create a new face element in the index
        /// </summary>
        /// <param name="faceInstance">the instance of a face in videos/insights/faces/instances</param>
        /// <param name="face">the face</param>
        public Face(JsonElement faceInstance, JsonElement face)
        {
            Id = face.GetProperty("id").GetInt32();
            Name = face.GetProperty("name").GetString();
            Start = Utils.GetTimeSpan(faceInstance, "start");
            End = Utils.GetTimeSpan(faceInstance, "end");
            Confidence = face.GetProperty("confidence").GetDouble();
            
            ThumbnailId = face.GetProperty("thumbnailId").GetString();

            if (face.TryGetProperty("knownPersonId", out JsonElement value2))
            {
                KnownPersonId = value2.GetString();
            }
        }

        public int Id { get; set; }

        [IsSearchable, IsFilterable, IsFacetable]
        public string Name { get; set; }

        [IsFacetable, IsFilterable]
        public string KnownPersonId { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        [IsFilterable]
        public double Confidence { get; set; }

        [IsFilterable]
        public string ThumbnailId { get; set; }

    }
}
