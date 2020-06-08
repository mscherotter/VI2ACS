using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public sealed class NamedLocation
    {
        [IsFilterable, IsFacetable]
        public string Name { get; set; }

        [IsFacetable, IsFilterable]
        public string ReferenceId { get; set; }

        public string ReferenceUrl { get; set; }

        [IsSearchable]
        public string Description { get; set; }

        public double Confidence { get; set; }

        [IsFilterable]
        public bool IsCustom { get; set; }
    }
}
