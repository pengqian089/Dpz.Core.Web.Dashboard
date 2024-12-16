using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class AudioService:IAudioService
    {
        private readonly IHttpService _httpService;

        public AudioService(
            IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IPagedList<AudioModel>> GetPageAsync(int pageIndex, int pageSize)
        {
            return await _httpService.GetPageAsync<AudioModel>("/api/Audio", pageIndex, pageSize);
        }

        public async Task DeleteAsync(string id)
        {
            await _httpService.DeleteAsync($"/api/Audio/{id}");
        }
    }
}