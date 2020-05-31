namespace VIToACS.Configurations
{
    public class AzureBlobWriterConfig
    {
        public string ConnectionString { get; set; }
        public string ScenesContainer { get; set; }
        public string TempUploadFilePath { get; set; }
        public string ThumbnailsContainer { get; set; }
    }
}
