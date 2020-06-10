using Microsoft.Azure.Search;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace VIToACS.Models
{
    public sealed class VisualContentModeration
    {
        public int Id { get; set; }
        
        [IsFilterable] 
        public double AdultScore { get; set; }

        [IsFilterable]
        public double RacyScore { get; set; }
    }
}
