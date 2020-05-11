namespace VIToACS.Configurations
{
    public class ReaderConfig
    {
        public string Type { get; set; }
        public AzureBlobReaderConfig AzureBlob { get; set; }
        public FileStreamReaderConfig FileStream { get; set; }
    }
}
