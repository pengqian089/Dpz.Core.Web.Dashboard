using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class AudioService(IHttpService httpService) : IAudioService
{
    public async Task<IPagedList<AudioModel>> GetPageAsync(int pageIndex, int pageSize)
    {
        return await httpService.GetPageAsync<AudioModel>("/api/Audio", pageIndex, pageSize);
    }

    public async Task DeleteAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Audio/{id}");
    }
}
