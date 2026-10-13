using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Management.Helper;
using Dpz.Core.Web.Management.Models;

namespace Dpz.Core.Web.Management.Service;

public interface IDanmakuService
{
    Task<IPagedList<DanmakuModel>> GetPageAsync(
        string? text = "",
        string? group = "",
        int pageIndex = 1,
        int pageSize = 10
    );

    Task<List<string>> GetGroupsAsync();

    Task DeleteAsync(params string[] id);

    Task ImportAcfunAsync(MultipartFormDataContent content);

    Task ImportBilibiliAsync(MultipartFormDataContent content);
}
