using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class AccountService : IAccountService
    {
        private readonly IHttpService _httpService;

        public AccountService(
            IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IPagedList<UserInfo>> GetPageAsync(string account = null, int pageIndex = 1,
            int pageSize = 10)
        {
            return await _httpService.GetPageAsync<UserInfo>("/api/Account", pageIndex, pageSize, new {account});
        }

        public async Task CreateAccountAsync(string account, string name, string password = null)
        {
            await _httpService.PostAsync("/api/Account", new {account, name, password});
        }

        public async Task<UserInfo> GetUserAsync(string account)
        {
            return await _httpService.GetAsync<UserInfo>($"/api/Account/{account}");
        }

        public async Task EnableAsync(string account)
        {
            await _httpService.PatchAsync($"/api/Account/{account}");
        }

        public async Task ChangePasswordAsync(string account, string password)
        {
            await _httpService.PatchAsync($"/api/Account/change-password", new {account, password});
        }

        public async Task<bool> ExistsAsync(string account)
        {
            var result = await _httpService.GetAsync<Exists>($"/api/Account/exists/{account}");
            return result?.IsExists ?? false;
        }
    }
}