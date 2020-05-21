using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VIToACS.Models;

namespace VIToACS.Interfaces
{
    public interface IVideoIndexer
    {
        Task SaveIndexAsync(IInsightsReader reader, MediaAsset media);
        Task<MediaAssetResults> ListVideosAsync(int skip);
    }
}
