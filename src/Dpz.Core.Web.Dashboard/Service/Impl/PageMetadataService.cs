using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class PageMetadataService(IHttpService httpService, IHttpClientFactory httpClientFactory)
    : IPageMetadataService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ServerAPI");

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

    public async Task<IReadOnlyList<PageRouteDefinitionResponse>> GetRoutesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default
    )
    {
        var routes = await httpService.GetAsync<List<PageRouteDefinitionResponse>>(
            "/api/Seo/routes",
            new { activeOnly },
            cancellationToken: cancellationToken
        );
        return routes ?? [];
    }

    public async Task<PageMetadataResponse?> PreviewAsync(
        SeoPreviewRequest request,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/Seo/preview",
            request,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new FetchException(
                string.IsNullOrWhiteSpace(message) ? "SEO 预览失败" : message.Trim('"')
            );
        }

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PageMetadataResponse>(
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

    public Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        return httpService.PostAsync(
            "/api/Seo/refresh-cache",
            cancellationToken: cancellationToken
        );
    }
}
