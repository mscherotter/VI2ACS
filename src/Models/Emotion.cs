using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Emotion
    {
        public int Id { get; set; }

        [IsFilterable, IsFacetable]
        public string Type { get; set; }

        public double Start { get; set; }

        public double End { get; set; }
    }
}
