using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public sealed class Label
    {
        public int Id { get; set; }

        [IsFilterable, IsFacetable]
        public string Name { get; set; }

        [IsFilterable, IsFacetable]
        public string ReferenceId { get; set; }

        [IsFilterable, IsFacetable]
        public string Language { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        [IsFilterable]
        public double Confidence { get; set; }
    }
}
