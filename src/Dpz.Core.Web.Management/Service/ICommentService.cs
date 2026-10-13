using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Management.Helper;
using Dpz.Core.Web.Management.Models;
using Dpz.Core.Web.Management.Models.Response;

namespace Dpz.Core.Web.Management.Service;

public interface ICommentService
{
    Task<IPagedList<CommentModel>> GetPageAsync(
        CommentNode? node,
        string relation,
        int pageIndex = 1,
        int pageSize = 15
    );

    Task ClearAsync(string id);

    Task<List<CommentRelationResponse>> GetArticleRelationAsync();

    Task<List<CommentRelationResponse>> CodeRelationAsync();

    Task<List<CommentRelationResponse>> OtherRelationAsync();
}
