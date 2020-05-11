namespace VIToACS.Configurations
{
    public class WriterConfig
    {
        public string Type { get; set; }
        public AzureBlobWriterConfig AzureBlob { get; set; }
        public FileStreamWriterConfig FileStream { get; set; }
    }
}
