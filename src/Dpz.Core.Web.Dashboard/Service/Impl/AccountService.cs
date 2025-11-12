using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class AccountService(IHttpService httpService) : IAccountService
{
    public async Task<IPagedList<UserInfo>> GetPageAsync(
        string account = null,
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        return await httpService.GetPageAsync<UserInfo>(
            "/api/Account",
            pageIndex,
            pageSize,
            new { account }
        );
    }

    public async Task CreateAccountAsync(string account, string name, string password = null)
    {
        await httpService.PostAsync(
            "/api/Account",
            new
            {
                account,
                name,
                password,
            }
        );
    }

    public async Task<UserInfo> GetUserAsync(string account)
    {
        return await httpService.GetAsync<UserInfo>($"/api/Account/{account}");
    }

    public async Task EnableAsync(string account)
    {
        await httpService.PatchAsync($"/api/Account/{account}");
    }

    public async Task ChangePasswordAsync(string account, string password)
    {
        await httpService.PatchAsync($"/api/Account/change-password", new { account, password });
    }

    public async Task<bool> ExistsAsync(string account)
    {
        var result = await httpService.GetAsync<Exists>($"/api/Account/exists/{account}");
        return result?.IsExists ?? false;
    }

    public Task<IPagedList<AccountLoginHistoryResponse>> GetAccountLoginHistoryAsync(
        AccountLoginHistoryRequest request,
        int pageIndex,
        int pageSize
    )
    {
        return httpService.GetPageAsync<AccountLoginHistoryResponse>(
            "/api/Account/history/login",
            pageIndex,
            pageSize,
            request
        );
    }
}
