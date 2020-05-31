using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Sentiment
    {
        [IsFilterable, IsFacetable]
        public string SentimentType { get; set; }

        public double AverageScore { get; set; }

        public double Start { get; set; }

        public double End { get; set; }
    }
}
