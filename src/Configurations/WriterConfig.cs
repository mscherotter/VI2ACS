namespace VIToACS.Configurations
{
    public class WriterConfig
    {
        public string Type { get; set; }
        public string ScenesDocumentPrefix { get; set; }
        public string ThumbnailsDocumentPrefix { get; set; }
        public AzureBlobWriterConfig AzureBlob { get; set; }
        public FileStreamWriterConfig FileStream { get; set; }
    }
}
