﻿using Microsoft.Azure.Search;
using System.Collections.Generic;

namespace VIToACS.Models
{
    public class Shot
    {
        [IsFilterable]
        public int Id { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        [IsFacetable, IsFilterable]
        public List<string> Tags { get; set; }

        public List<KeyFrame> KeyFrames { get; set; }
    }
}
