using Microsoft.Azure.Search;
using System.Text.Json;

namespace VIToACS.Models
{
    public class Keyword
    {
        [IsSearchable, IsFilterable, IsFacetable]
        public string Text { get; set; }

        [IsFilterable]
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the language
        /// </summary>
        [IsFilterable, IsFacetable]
        public string Language { get; set; }

        public static Keyword Create(JsonElement item )
        {
            return new Keyword
            {
                Text = item.GetProperty("text").GetString(),
                Confidence = item.GetProperty("confidence").GetDouble(),
                Language = item.GetProperty("language").GetString()
            };
        }
    }
}
