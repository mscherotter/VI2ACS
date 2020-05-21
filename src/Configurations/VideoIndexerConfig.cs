using System;
using System.Collections.Generic;
using System.Text;

namespace VIToACS.Configurations
{
    public class VideoIndexerConfig
    {
        public string Location { get; set; }
        public string AccountId { get; set; }
        public int PageSize { get; set; }
        public string SubscriptionKey { get; set; }
    }
}
