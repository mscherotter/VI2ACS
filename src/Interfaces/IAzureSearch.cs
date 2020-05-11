using Microsoft.Azure.Search;

namespace VIToACS.Interfaces
{
    public interface IAzureSearch
    {
        void CreateSceneIndex();
        void CreateThumbnailIndex();
        SearchServiceClient GetClient();
    }
}
