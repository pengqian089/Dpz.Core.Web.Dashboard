using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class MumbleService:IMumbleService
    {
        private readonly IHttpService _httpService;

        public MumbleService(
            IHttpService httpService)
        {
            _httpService = httpService;
        }
        
        public async Task<IPagedList<MumbleModel>> GetPageAsync(string content = null, int pageIndex = 1, int pageSize = 10)
        {
            return await _httpService.GetPageAsync<MumbleModel>("/api/Mumble", pageIndex, pageSize, new {content});
        }

        public async Task CreateAsync(string markdown, string htmlContent)
        {
            await _httpService.PostAsync("/api/Mumble", new {markdown, htmlContent});
        }

        public async Task EditAsync(string id, string markdown, string htmlContent)
        {
            await _httpService.PatchAsync("/api/Mumble", new {id, markdown, htmlContent});
        }

        public async Task<MumbleModel> GetMumbleAsync(string id)
        {
            return await _httpService.GetAsync<MumbleModel>($"/api/Mumble/{id}");
        }

        public async Task DeleteAsync(string id)
        {
            await _httpService.DeleteAsync($"/api/Mumble/{id}");
        }

        public async Task DeleteCommentAsync(string id,string commentId)
        {
            await _httpService.DeleteAsync($"/api/Mumble/{id}/comment/{commentId}");
        }

        public async Task<IPagedList<MumbleCommentModel>> GetCommentPageAsync(string id, string content = null, int pageIndex = 1, int pageSize = 10)
        {
            return await _httpService.GetPageAsync<MumbleCommentModel>($"/api/Mumble/comment/{id}", pageIndex, pageSize,
                new {content});
        }

        public async Task<string> UploadAsync(MultipartFormDataContent content)
        {
            var result = await _httpService.PostFileAsync<string>("/api/Mumble/upload",content);
            var json = JsonNode.Parse(result);
            return json?["url"]?.GetValue<string>();
        }
    }
}