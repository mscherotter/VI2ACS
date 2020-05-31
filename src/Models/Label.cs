using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Label
    {
        public int Id { get; set; }

        [IsFilterable, IsFacetable]
        public string Name { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        public double Confidence { get; set; }
    }
}
