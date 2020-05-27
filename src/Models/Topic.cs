using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Topic
    {
        [IsSearchable, IsFacetable, IsFilterable]
        public string Name { get; set; }
        public double Confidence { get; set; }
    }
}
