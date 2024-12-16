using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class MusicService : IMusicService
    {
        private readonly IHttpService _httpService;

        public MusicService(
            IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IPagedList<MusicModel>> GetPageAsync(string title = null, int pageIndex = 1, int pageSize = 10)
        {
            return await _httpService.GetPageAsync<MusicModel>("/api/Music", pageIndex, pageSize, new { title });
        }

        public async Task EditLyricAsync(MultipartFormDataContent content)
        {
            await _httpService.PostFileAsync("/api/Music", content, HttpMethod.Patch);
        }

        public async Task EditInformationAsync(MultipartFormDataContent content)
        {
            await _httpService.PostFileAsync("/api/Music/information", content, HttpMethod.Patch);
        }

        public async Task AddMusicAsync(MultipartFormDataContent content)
        {
            await _httpService.PostFileAsync("/api/Music", content);
        }

        public async Task<MusicModel> GetMusicAsync(string id)
        {
            return await _httpService.GetAsync<MusicModel>($"/api/Music/{id}");
        }

        public async Task DeleteAsync(string id)
        {
            await _httpService.DeleteAsync($"/api/Music/{id}");
        }

        public async Task<string> GetLyricAsync(string id)
        {
            return await _httpService.GetAsync<string>($"/api/Music/lrc/{id}");
        }

        public async Task<List<string>> GetGroupsAsync()
        {
            return await _httpService.GetAsync<List<string>>("/api/Music/groups");
        }
    }
}