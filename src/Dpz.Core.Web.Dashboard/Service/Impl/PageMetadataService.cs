using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class PageMetadataService(IHttpService httpService) : IPageMetadataService
{
    public Task<IPagedList<PageMetadataResponse>> GetPageAsync(
        string? searchText = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        return httpService.GetPageAsync<PageMetadataResponse>(
            "/api/Seo",
            pageIndex,
            pageSize,
            new { searchText },
            cancellationToken
        );
    }

    public Task<PageMetadataResponse?> GetAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        return httpService.GetAsync<PageMetadataResponse?>(
            $"/api/Seo/{id}",
            cancellationToken: cancellationToken
        );
    }

    public Task<PageMetadataResponse?> GetByRoute(
        string controller,
        string action,
        string? routeId = null,
        CancellationToken cancellationToken = default
    )
    {
        return httpService.GetAsync<PageMetadataResponse?>(
            "/api/Seo/preview",
            new
            {
                controller,
                action,
                routeId,
            },
            cancellationToken: cancellationToken
        );
    }

    public Task SaveAsync(SeoSaveRequest request, CancellationToken cancellationToken = default)
    {
        return httpService.PostAsync("/api/Seo", request, cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return httpService.DeleteAsync($"/api/Seo/{id}", cancellationToken: cancellationToken);
    }

    public Task RefreshCacheAsync(CancellationToken cancellationToken)
    {
        return httpService.PostAsync("/api/Seo/refresh-cache", cancellationToken: cancellationToken);
    }
}
