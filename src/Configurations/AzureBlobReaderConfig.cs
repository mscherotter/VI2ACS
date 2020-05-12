namespace VIToACS.Configurations
{
    public class AzureBlobReaderConfig
    {
        public string ConnectionString { get; set; }
        public string InsightsContainer { get; set; }
        public string TempDownloadFilePath { get; set; }
    }
}
