using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models
{
    /// <summary>
    /// Custom vision prediction model
    /// </summary>
    public sealed class CustomVision
    {
        /// <summary>
        /// Gets or sets the classifier id
        /// </summary>
        [IsFilterable, IsFacetable]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the predictions
        /// </summary>
        public List<Prediction> Predictions { get; set; }
    }
}
