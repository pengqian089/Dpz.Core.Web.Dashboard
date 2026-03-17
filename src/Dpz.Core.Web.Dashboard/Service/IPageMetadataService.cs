using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IPageMetadataService
{
    Task<IPagedList<PageMetadataResponse>> GetPageAsync(
        string? searchText = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );

    Task<PageMetadataResponse?> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<PageMetadataResponse?> GetByRoute(
        string controller,
        string action,
        string? routeId = null,
        CancellationToken cancellationToken = default
    );

    Task SaveAsync(SeoSaveRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
