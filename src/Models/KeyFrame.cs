using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models
{
    public class KeyFrame
    {
        public int Id { get; set; }
        
        [IsFilterable, IsFacetable]
        public string ThumbnailId { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        /// <summary>
        /// Gets or sets the custom vision models
        /// </summary>
        public List<CustomVision> CustomVisionModels { get; set; }
    }
}
