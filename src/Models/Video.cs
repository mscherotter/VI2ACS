using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models
{
    public class Video
    {
        [IsFilterable, IsFacetable]
        public string Id { get; set; }

        public string ThumbnailId { get; set; }

        [IsFilterable, IsFacetable]
        public bool DetectSourceLanguage { get; set; }

        [IsFilterable, IsFacetable]
        public List<string> SourceLanguages { get; set; }

        /// <summary>
        /// Gets or sets the show name
        /// </summary>
        [IsFilterable, IsFacetable]
        public string ShowName { get; set; }

        /// <summary>
        /// Gets or sets the episode name
        /// </summary>
        [IsFilterable, IsFacetable]
        public string EpisodeName { get; set; }
        
        /// <summary>
        /// Gets or sets the season number
        /// </summary>
        [IsFacetable, IsFilterable]
        public int SeasonNumber { get; set; }

        /// <summary>
        /// Gets or sets the eposide number
        /// </summary>
        [IsFacetable, IsFilterable]
        public int EpisodeNumber { get; set; }
    }
}
