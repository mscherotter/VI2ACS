﻿using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models
{
    public class Thumbnail
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }

        public Video Video { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        [IsFacetable]
        public List<string> Faces { get; set; }

        [IsFacetable]
        public List<string> Labels { get; set; }

        public List<Ocr> Ocr { get; set; }

        public List<Keyword> Keywords { get; set; }

        [IsFacetable]
        public List<string> ShotTags { get; set; }

        public List<Topic> Topics { get; set; }

        public Playlist Playlist { get; set; }
    }
}