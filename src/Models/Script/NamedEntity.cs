using Microsoft.Azure.Search;

namespace VIToACS.Models.Script
{
    /// <summary>
    /// Named entity
    /// </summary>
    public sealed class NamedEntity
    {
        /// <summary>
        /// Gets or sets the type of the named entity
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the named entity
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string Name { get; set; }
    }
}
