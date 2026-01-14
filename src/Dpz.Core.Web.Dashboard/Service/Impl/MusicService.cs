using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class MusicService(IHttpService httpService) : IMusicService
{
    public async Task<IPagedList<MusicModel>> GetPageAsync(
        string? title = null,
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        return await httpService.GetPageAsync<MusicModel>(
            "/api/Music",
            pageIndex,
            pageSize,
            new { title }
        );
    }

    public async Task EditLyricAsync(MultipartFormDataContent content)
    {
        await httpService.PostFileAsync("/api/Music", content, HttpMethod.Patch);
    }

    public async Task EditInformationAsync(MultipartFormDataContent content)
    {
        await httpService.PostFileAsync("/api/Music/information", content, HttpMethod.Patch);
    }

    public async Task AddMusicAsync(MultipartFormDataContent content)
    {
        await httpService.PostFileAsync("/api/Music", content);
    }

    public async Task<MusicModel?> GetMusicAsync(string id)
    {
        return await httpService.GetAsync<MusicModel>($"/api/Music/{id}");
    }

    public async Task DeleteAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Music/{id}");
    }

    public async Task<string> GetLyricAsync(string id)
    {
        return await httpService.GetAsync<string>($"/api/Music/lrc/{id}");
    }

    public async Task<List<string>> GetGroupsAsync()
    {
        return await httpService.GetAsync<List<string>>("/api/Music/groups");
    }
}
