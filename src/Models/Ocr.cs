using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Ocr
    {
        [IsSearchable]
        public string Text { get; set; }
        [IsFilterable]
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the language
        /// </summary>
        [IsFacetable, IsFilterable]
        public string Language { get; set; }
    }
}
