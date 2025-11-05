using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class CommentService(IHttpService httpService) : ICommentService
{
    public async Task<IPagedList<CommentModel>> GetPageAsync(
        CommentNode? node,
        string relation,
        int pageIndex = 1,
        int pageSize = 15
    )
    {
        return await httpService.GetPageAsync<CommentModel>(
            "/api/Comment",
            pageIndex,
            pageSize,
            new { node, relation }
        );
    }

    public async Task ClearAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Comment/{id}");
    }

    public async Task<List<CommentRelationResponse>> GetArticleRelationAsync()
    {
        return await httpService.GetAsync<List<CommentRelationResponse>>(
            "/api/Comment/relation/article"
        );
    }

    public async Task<List<CommentRelationResponse>> CodeRelationAsync()
    {
        return await httpService.GetAsync<List<CommentRelationResponse>>(
            "/api/Comment/relation/code"
        );
    }

    public async Task<List<CommentRelationResponse>> OtherRelationAsync()
    {
        return await httpService.GetAsync<List<CommentRelationResponse>>(
            "/api/Comment/relation/other"
        );
    }
}
