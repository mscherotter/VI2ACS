using Microsoft.Azure.Search;

namespace VIToACS.Models
{
    public class Ocr
    {
        [IsSearchable]
        public string Text { get; set; }
        public double Confidence { get; set; }
    }
}
