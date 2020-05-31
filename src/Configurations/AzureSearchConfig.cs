namespace VIToACS.Configurations
{
    public class AzureSearchConfig
    {
        public string Name { get; set; }
        public string AdminKey { get; set; }
        public bool DeleteIndexIfExists { get; set; }
        public string SceneIndexName { get; set; }
        public string ThumbnailIndexName { get; set; }
    }
}
