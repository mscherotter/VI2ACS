using System;
using System.Collections.Generic;
using System.Text;

namespace VIToACS.Interfaces
{
    public interface IParsedDocument<T>
    {
        public string FileName { get; set; }
        public string ParsedScenesJson { get; set; }
        public string ParsedThumbnailsJson { get; set; }
        public IEnumerable<T> Scenes { get; set; }
        public IEnumerable<Models.Thumbnail> Thumbnails { get; set; }
    }
}
