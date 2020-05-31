using System.Collections.Generic;
using VIToACS.Models;

namespace VIToACS.Interfaces
{
    public interface IAzureSearch
    {
        void CreateSceneIndex();
        void CreateThumbnailIndex();
        void UploadSceneDocuments(IEnumerable<Scene> sceneDocuments);
        void UploadThumbnailDocuments(IEnumerable<Thumbnail> thumbnailDocuments);
    }
}
