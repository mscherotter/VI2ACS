﻿using System.Collections.Generic;
using VIToACS.Interfaces;

namespace VIToACS.Models
{
    public class ParsedDocument<T>: IParsedDocument<T>
    {
        public string FileName { get; set; }
        public string ParsedScenesJson { get; set; }
        public string ParsedThumbnailsJson { get; set; }
        public IEnumerable<T> Scenes { get; set; }
        public IEnumerable<Thumbnail> Thumbnails { get; set; }
    }
}
