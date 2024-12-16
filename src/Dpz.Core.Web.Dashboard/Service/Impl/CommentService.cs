using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class CommentService:ICommentService
{
    private readonly IHttpService _httpService;

    public CommentService(IHttpService httpService)
    {
        _httpService = httpService;
    }
    
    public async Task<IPagedList<CommentModel>> GetPageAsync(CommentNode? node, string relation, int pageIndex = 1, int pageSize = 15)
    {
        return await _httpService.GetPageAsync<CommentModel>("/api/Comment", pageIndex, pageSize, new {node, relation});
    }

    public async Task ClearAsync(string id)
    {
        await _httpService.DeleteAsync($"/api/Comment/{id}");
    }

    public async Task<IDictionary<string, string>> GetArticleRelationAsync()
    {
        return await _httpService.GetAsync<Dictionary<string, string>>("/api/Comment/relation/article");
    }

    public async Task<IDictionary<string, string>> CodeRelationAsync()
    {
        return await _httpService.GetAsync<Dictionary<string, string>>("/api/Comment/relation/code");
    }

    public async Task<IDictionary<string, string>> OtherRelationAsync()
    {
        return await _httpService.GetAsync<Dictionary<string, string>>("/api/Comment/relation/other");
    }
}