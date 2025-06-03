using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Request;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class DynamicPageService : IDynamicPageService
    {
        private readonly IHttpService _httpService;

        public DynamicPageService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IPagedList<DynamicPageListModel>> GetPageAsync(
            string name = null,
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            return await _httpService.GetPageAsync<DynamicPageListModel>(
                "/api/DynamicPage",
                pageIndex,
                pageSize,
                new { id = name }
            );
        }

        public async Task<DynamicPageModel> GetDynamicPageAsync(string id)
        {
            return await _httpService.GetAsync<DynamicPageModel>($"/api/DynamicPage/{id}");
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var result = await _httpService.GetAsync<Exists>($"/api/DynamicPage/exists/{id}");
            return result?.IsExists ?? false;
        }

        public async Task CreateDynamicPage(SaveDynamicRequest request)
        {
            await _httpService.PostAsync("/api/DynamicPage", request);
        }

        public async Task EditDynamicPage(SaveDynamicRequest request)
        {
            await _httpService.PatchAsync($"/api/DynamicPage/", request);
        }

        public async Task DeleteAsync(string id)
        {
            await _httpService.DeleteAsync($"/api/DynamicPage/{id}");
        }
    }
}
