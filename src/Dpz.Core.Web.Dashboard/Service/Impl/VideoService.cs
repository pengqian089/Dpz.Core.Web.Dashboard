using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class VideoService(IHttpService httpService) : IVideoService
{
    public async Task<List<VideoModel>> GetVideosAsync()
    {
        return await httpService.GetAsync<List<VideoModel>>("/api/Video/details") ?? [];
    }

    public async Task SaveVideoInformationAsync(VideoModel model)
    {
        await httpService.PostAsync("/api/Video", model);
    }

    public async Task<VideoMetaDataModel?> GetVideoMetadataAsync(string id)
    {
        return await httpService.GetAsync<VideoMetaDataModel>($"/api/Video/meta/{id}");
    }

    public Task SetVideoScreenshotAsync(string id, double seconds)
    {
        return httpService.PatchAsync($"/api/Video/screenshot/{id}", new { seconds });
    }
}
