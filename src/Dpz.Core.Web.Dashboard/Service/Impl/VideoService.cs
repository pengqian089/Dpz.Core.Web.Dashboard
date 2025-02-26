﻿using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class VideoService : IVideoService
{
    private readonly IHttpService _httpService;

    public VideoService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<IList<VideoModel>> GetVideosAsync()
    {
        return await _httpService.GetAsync<List<VideoModel>>("/api/Video/details");
    }

    public async Task SaveVideoInformationAsync(VideoModel model)
    {
        await _httpService.PostAsync("/api/Video", model);
    }

    public async Task<VideoMetaDataModel> GetVideoMetadataAsync(string id)
    {
        return await _httpService.GetAsync<VideoMetaDataModel>($"/api/Video/meta/{id}");
    }

    public Task SetVideoScreenshotAsync(string id, double seconds)
    {
        return _httpService.PatchAsync($"/api/Video/screenshot/{id}", new { seconds });
    }
}
