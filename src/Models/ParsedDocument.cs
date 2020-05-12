using System.Collections.Generic;

namespace VIToACS.Models
{
    public class ParsedDocument
    {
        public string FileName { get; set; }
        public string ParsedScenesJson { get; set; }
        public string ParsedThumbnailsJson { get; set; }
        public IEnumerable<Scene> Scenes { get; set; }
        public IEnumerable<Thumbnail> Thumbnails { get; set; }
    }
}
