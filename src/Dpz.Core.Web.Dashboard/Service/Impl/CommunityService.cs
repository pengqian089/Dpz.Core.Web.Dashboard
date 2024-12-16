using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class CommunityService : ICommunityService
    {
        private readonly IHttpService _httpService;

        public CommunityService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<string> GetLogsAsync()
        {
            return await _httpService.GetAsync<string>("/api/Community/logs");
        }

        public async Task<SummaryInformation> GetSummaryAsync()
        {
            return await _httpService.GetAsync<SummaryInformation>("/api/Community/summary");
        }

        public async Task<string> GetFooterAsync()
        {
            return await _httpService.GetAsync<string>("/api/Community/footer");
        }

        public async Task SaveFooterAsync(string content)
        {
            await _httpService.PostAsync("/api/Community/footer", new {content});
        }

        public async Task<SetupInfo> GetTwoFactorSetupInfoAsync()
        {
            return await _httpService.GetAsync<SetupInfo>("/api/Community/bind-two-factor");
        }

        public async Task BindTwoFactorAsync(string pinCode)
        {
            await _httpService.PostAsync("/api/Community/bind-two-factor", new {pinCode});
        }

        public async Task UnbindTwoFactorAsync(string pinCode)
        {
            await _httpService.PostAsync("/api/Community/unbind-two-factor", new {pinCode});
        }

        public async Task<bool> CheckBindTwoFactorAsync()
        {
            return await _httpService.GetAsync<bool>("/api/Community/check-bind-two-factor");
        }
    }
}