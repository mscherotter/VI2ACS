using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Transcript
    {
        public int Id { get; set; }

        [IsSearchable]
        public string Text { get; set; }

        [IsFilterable, IsFacetable]
        public string Language { get; set; }

        public double Start { get; set; }

        public double End { get; set; }
    }
}
