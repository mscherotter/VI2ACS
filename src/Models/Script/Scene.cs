using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models.Script
{
    /// <summary>
    ///  A scene from the script parser
    /// </summary>
    public sealed class Scene
    {
        /// <summary>
        /// Gets or sets the unique ID of the scene
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the set name
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string Set { get; set; }

        /// <summary>
        /// Gets or sets the elements in the scene
        /// </summary>
        public IEnumerable<Element> Elements { get; set; }

        /// <summary>
        /// Gets the interior or exterior value
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string InteriorExterior { get; set; }

        /// <summary>
        /// Gets or sets the time (night or day)
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string Time { get; set; }

        /// <summary>
        /// Gets or sets the scene location
        /// </summary>
        [IsSearchable, IsFilterable, IsFacetable]
        public string Location { get; set; }
    }
}
