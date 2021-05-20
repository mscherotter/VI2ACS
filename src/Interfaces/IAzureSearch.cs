using System.Collections.Generic;
using VIToACS.Models;

namespace VIToACS.Interfaces
{
    /// <summary>
    /// Azure Search interface
    /// </summary>
    public interface IAzureSearch
    {
        void CreateSceneIndex();
        void CreateThumbnailIndex();
        /// <summary>
        /// Upload scene documents
        /// </summary>
        /// <typeparam name="T">the type of scene</typeparam>
        /// <param name="sceneDocuments">the scene documents</param>
        void UploadSceneDocuments<T>(IEnumerable<T> sceneDocuments);

        void UploadThumbnailDocuments(IEnumerable<Thumbnail> thumbnailDocuments);
    }
}
