using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class KeyFrame
    {
        public int Id { get; set; }
        
        [IsFilterable, IsFacetable]
        public string ThumbnailId { get; set; }

        public double Start { get; set; }

        public double End { get; set; }
    }
}
