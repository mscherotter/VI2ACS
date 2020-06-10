using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    /// <summary>
    /// Tag Name probability for classifiers
    /// </summary>
    public sealed class Prediction
    {
        /// <summary>
        /// Gets or sets the tag name
        /// </summary>
        [IsFacetable, IsFilterable]
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the probability 0-1
        /// </summary>
        [IsFilterable]
        public double Probability { get; set; }
    }
}
