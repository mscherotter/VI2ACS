using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models.Script
{
    /// <summary>
    /// Scene element
    /// </summary>
    public sealed class Element
    {
        /// <summary>
        /// Gets or sets the unique ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the element (Character or Action)
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string Type { get; set; }
        
        /// <summary>
        /// Gets the description of the character or action
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable] 
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the dialog
        /// </summary>
        [IsSearchable] 
        public string Dialogue { get; set; }

        /// <summary>
        /// Gets or sets the parenthetical
        /// </summary>
        [IsSearchable]
        public string Parenthetical { get; set; }

        /// <summary>
        /// Gets or sets the nouns
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public IEnumerable<string> Nouns { get; set; }

        /// <summary>
        /// Gets or sets the named entities
        /// </summary>
        public IEnumerable<NamedEntity> NamedEntities { get; set; }
    }
}
