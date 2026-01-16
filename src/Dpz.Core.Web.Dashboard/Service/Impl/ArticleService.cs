using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class ArticleService(IHttpService httpService) : IArticleService
{
    public async Task<IPagedList<ArticleModel>> GetPageAsync(
        int pageIndex,
        int pageSize,
        string? tag,
        string? title
    )
    {
        return await httpService.GetPageAsync<ArticleModel>(
            "/api/Article",
            pageIndex,
            pageSize,
            new { tags = tag, title }
        );
    }

    public async Task PublishAsync(ArticlePublishRequest request)
    {
        await httpService.PostAsync("/api/Article", request);
    }

    public async Task<ArticleModel?> GetArticleAsync(string id)
    {
        return await httpService.GetAsync<ArticleModel>($"/api/Article/{id}");
    }

    public async Task<List<string>> GetTagsAsync()
    {
        return await httpService.GetAsync<List<string>>("/api/Article/tags/all") ?? [];
    }

    public async Task<List<ArticleModel>> GetNewArticlesAsync()
    {
        return await httpService.GetAsync<List<ArticleModel>>("/api/Article/news") ?? [];
    }

    public async Task<List<ArticleModel>> GetTopViewArticlesAsync()
    {
        return await httpService.GetAsync<List<ArticleModel>>("/api/Article/topView") ?? [];
    }

    public async Task EditAsync(ArticleEditRequest request)
    {
        await httpService.PatchAsync("/api/Article", request);
    }

    public async Task DeleteAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Article/{id}");
    }

    public async Task<bool> ExistsAsync(string title)
    {
        var result = await httpService.GetAsync<Exists>($"/api/Article/exists/{title}");
        return result?.IsExists ?? false;
    }

    public async Task<string?> UploadAsync(MultipartFormDataContent content)
    {
        var result = await httpService.PostFileAsync<string>("/api/Article/upload", content);
        if (result == null)
        {
            return null;
        }
        try
        {
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
