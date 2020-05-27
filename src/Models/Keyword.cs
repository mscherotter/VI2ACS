using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Keyword
    {
        [IsSearchable, IsFilterable]
        public string Text { get; set; }
        public double Confidence { get; set; }
    }
}
