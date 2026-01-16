using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class TimelineService(IHttpService httpService) : ITimelineService
{
    public async Task<IPagedList<TimelineModel>> GetPageAsync(
        string? title = null,
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        return await httpService.GetPageAsync<TimelineModel>(
            "/api/Timeline/page",
            pageIndex,
            pageSize,
            new { content = title }
        );
    }

    public async Task CreateTimelineAsync(TimelineCreateRequest request)
    {
        await httpService.PostAsync("/api/Timeline", request);
    }

    public async Task EditTimelineAsync(TimelineEditRequest request)
    {
        await httpService.PatchAsync("/api/Timeline", request);
    }

    public async Task<TimelineModel?> GetTimelineAsync(string id)
    {
        return await httpService.GetAsync<TimelineModel>($"/api/Timeline/{id}");
    }

    public async Task DeleteAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Timeline/{id}");
    }

    public async Task<string?> UploadAsync(MultipartFormDataContent content)
    {
        var result = await httpService.PostFileAsync<string>("/api/Timeline/upload", content);
        try
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return null;
            }
            var json = JsonNode.Parse(result);
            return json?["url"]?.GetValue<string>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}
