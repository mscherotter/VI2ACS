using System;
using System.Collections.Generic;
using System.Text;

namespace VIToACS.Models
{
    public class MediaAsset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Partition { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public string UserName { get; set; }
        public string SourceLanguage { get; set; }
        public int DurationInSeconds { get; set; }
    }
}
