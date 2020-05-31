using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models
{
    public class Video
    {
        [IsFilterable]
        public string Id { get; set; }

        public string ThumbnailId { get; set; }

        [IsFilterable, IsFacetable]
        public bool DetectSourceLanguage { get; set; }

        [IsFilterable, IsFacetable]
        public List<string> SourceLanguages { get; set; }
    }
}
