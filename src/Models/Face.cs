using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Face
    {
        public int Id { get; set; }

        [IsSearchable, IsFilterable, IsFacetable]
        public string Name { get; set; }

        [IsFacetable]
        public string KnownPersonId { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        public double Confidence { get; set; }
    }
}
