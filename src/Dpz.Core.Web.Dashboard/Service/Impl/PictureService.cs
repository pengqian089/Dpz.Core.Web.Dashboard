using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Pages.Article;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class PictureService:IPictureService
    {
        private readonly IHttpService _httpService;

        public PictureService(
            IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IPagedList<PictureResponseModel>> GetPageAsync(string tag = null, string description = null, int type = -1, int pageIndex = 1,
            int pageSize = 10)
        {
            return await _httpService.GetPageAsync<PictureResponseModel>("/api/Picture", pageIndex, pageSize,
                new {tag, description, type});
        }

        public async Task<PictureResponseModel> GetPictureAsync(string id)
        {
            return await _httpService.GetAsync<PictureResponseModel>($"/api/Picture/{id}");
        }

        public async Task UploadAsync(MultipartFormDataContent content)
        {
            await _httpService.PostFileAsync("/api/Picture",content);
        }

        public async Task EditAsync(object content)
        {
            await _httpService.PatchAsync("/api/Picture", content);
        }

        public async Task DeleteAsync(string id)
        {
            await _httpService.DeleteAsync($"/api/Picture/{id}");
        }

        public async Task<List<string>> GetTagsAsync()
        {
            return await _httpService.GetAsync<List<string>>("/api/Picture/tags");
        }
    }
}