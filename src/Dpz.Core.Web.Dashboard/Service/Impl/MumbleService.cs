using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class MumbleService(IHttpService httpService) : IMumbleService
{
    public async Task<IPagedList<MumbleModel>> GetPageAsync(
        string? content = null,
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        return await httpService.GetPageAsync<MumbleModel>(
            "/api/Mumble",
            pageIndex,
            pageSize,
            new { content }
        );
    }

    public async Task CreateAsync(string markdown, string htmlContent)
    {
        await httpService.PostAsync("/api/Mumble", new { markdown, htmlContent });
    }

    public async Task EditAsync(string id, string markdown)
    {
        await httpService.PatchAsync("/api/Mumble", new { id, markdown });
    }

    public async Task<MumbleModel?> GetMumbleAsync(string id)
    {
        return await httpService.GetAsync<MumbleModel>($"/api/Mumble/{id}");
    }

    public async Task DeleteAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Mumble/{id}");
    }

    public async Task<string?> UploadAsync(MultipartFormDataContent content)
    {
        var result = await httpService.PostFileAsync<string>("/api/Mumble/upload", content);
        if (result == null)
        {
            return null;
        }

        var json = JsonNode.Parse(result);
        return json?["url"]?.GetValue<string>();
    }
}
