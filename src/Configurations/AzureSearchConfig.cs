namespace VIToACS.Configurations
{
    public class AzureSearchConfig
    {
        public string Name { get; set; }
        public string AdminKey { get; set; }
        public bool DeleteIndexIfExists { get; set; }
        public string SceneIndexName { get; set; }
        public string ThumbnailIndexName { get; set; }

        /// <summary>
        /// Gets or sets the type to use, "ScriptParser" to use the script parser data instead of video indexer data
        /// </summary>
        public string Type { get; set; }
    }
}
