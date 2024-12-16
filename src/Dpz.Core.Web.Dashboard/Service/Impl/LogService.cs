using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class LogService : ILogService
{
    private readonly IHttpService _httpService;

    public LogService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<IPagedList<LogModel>> GetPageAsync(string logger = null, string level = null,
        string message = null, int pageIndex = 1,
        int pageSize = 15)
    {
        return await _httpService.GetPageAsync<LogModel>("/api/Logs", pageIndex, pageSize,
            new {logger, level, message});
    }

    public async Task<IList<string>> GetLoggerAsync()
    {
        return await _httpService.GetAsync<List<string>>("/api/Logs/loggers");
    }

    public async Task<IList<string>> GetLevelAsync()
    {
        return await _httpService.GetAsync<List<string>>("/api/Logs/levels");
    }

    public async Task<string> GetDetailAsync(string id)
    {
        return await _httpService.GetAsync<string>($"/api/Logs/{id}");
    }
}