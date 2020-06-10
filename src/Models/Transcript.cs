using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Transcript
    {
        public int Id { get; set; }

        [IsSearchable]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the confidence (0-1)
        /// </summary>
        [IsFilterable]
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the speakerId
        /// </summary>
        [IsFilterable, IsFacetable]
        public int SpeakerId { get; set; }

        [IsFilterable, IsFacetable]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the start in seconds
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        /// Gets or sets the end in seconds
        /// </summary>
        public double End { get; set; }
    }
}
