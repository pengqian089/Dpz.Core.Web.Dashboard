using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class CommunityService(IHttpService httpService) : ICommunityService
{
    public async Task<string> GetLogsAsync()
    {
        return await httpService.GetAsync<string>("/api/Community/logs") ?? "";
    }

    public async Task<SummaryInformation> GetSummaryAsync()
    {
        return await httpService.GetAsync<SummaryInformation>("/api/Community/summary")
            ?? new SummaryInformation();
    }

    public async Task<string> GetFooterAsync()
    {
        return await httpService.GetAsync<string>("/api/Community/footer") ?? "";
    }

    public async Task SaveFooterAsync(string content)
    {
        await httpService.PostAsync("/api/Community/footer", new { content });
    }

    public async Task<string> GetRobotsAsync()
    {
        return await httpService.GetAsync<string>("/api/Community/robots.txt") ?? "";
    }

    public async Task SaveRobotsAsync(string content)
    {
        await httpService.PostAsync("/api/Community/robots.txt", new { content });
    }
}
