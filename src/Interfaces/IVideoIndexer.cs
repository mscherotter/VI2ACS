using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VIToACS.Models;

namespace VIToACS.Interfaces
{
    public interface IVideoIndexer<T>
    {
        Task AddNewInsightsFileToReaderAsync(IInsightsReader<T> reader, MediaAsset media);
        bool IsDownloadInsightsEnabled();
        bool IsDownloadThumbnailsEnabled();
        Task<MediaAssetResults> ListVideosAsync(int skip);
        Task DownloadThumbnailAsync(IDocumentWriter writer, string fileName, string videoId, string thumbnailId);
    }
}
