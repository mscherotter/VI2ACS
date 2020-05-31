using System;
using System.Collections.Generic;
using System.Text;

namespace VIToACS.Configurations
{
    public class VideoIndexerConfig
    {
        public string AccessToken { get; set; }
        public string AccountId { get; set; }
        public bool DownloadInsights { get; set; }
        public bool DownloadThumbnails { get; set; }
        public string Location { get; set; }
        public int PageSize { get; set; }
        public string SubscriptionKey { get; set; }
    }
}
