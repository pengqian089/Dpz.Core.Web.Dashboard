using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class DanmakuService(IHttpService httpService) : IDanmakuService
{
    public async Task<IPagedList<DanmakuModel>> GetPageAsync(
        string? text = "",
        string? @group = "",
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        return await httpService.GetPageAsync<DanmakuModel>(
            "/api/Danmaku",
            pageIndex,
            pageSize,
            new { text, group }
        );
    }

    public async Task<List<string>> GetGroupsAsync()
    {
        return await httpService.GetAsync<List<string>>("/api/Danmaku/group") ?? [];
    }

    public async Task DeleteAsync(params string[] id)
    {
        await httpService.DeleteAsync("/api/Danmaku", id);
    }

    public async Task ImportAcfunAsync(MultipartFormDataContent content)
    {
        await httpService.PostFileAsync("/api/Danmaku/import/acfun", content);
    }

    public async Task ImportBilibiliAsync(MultipartFormDataContent content)
    {
        await httpService.PostFileAsync("/api/Danmaku/import/bilibili", content);
    }
}
