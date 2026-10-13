using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Management.Helper;
using Dpz.Core.Web.Management.Models.Request;
using Dpz.Core.Web.Management.Models.Response;

namespace Dpz.Core.Web.Management.Service.Impl;

public class MessageOutboxService(IHttpService httpService) : IMessageOutboxService
{
    public Task<IPagedList<MessageOutboxResponse>> GetPageAsync(
        MessageOutboxRequest request,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var query = new
        {
            request.Keyword,
            request.Status,
            request.MessageType,
            request.Exchange,
            request.RoutingKey,
            request.Source,
            StartTime = request.StartTime?.ToString("yyyy-MM-ddTHH:mm:ss"),
            EndTime = request.EndTime?.ToString("yyyy-MM-ddTHH:mm:ss"),
        };

        return httpService.GetPageAsync<MessageOutboxResponse>(
            "/api/MessageOutbox",
            pageIndex,
            pageSize,
            query,
            cancellationToken
        );
    }

    public Task<MessageOutboxFilterOptionsResponse?> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return httpService.GetAsync<MessageOutboxFilterOptionsResponse>(
            "/api/MessageOutbox/filter-options",
            cancellationToken: cancellationToken
        );
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return httpService.DeleteAsync(
            $"/api/MessageOutbox/{id}",
            cancellationToken: cancellationToken
        );
    }

    public Task ReconsumeAsync(string id, CancellationToken cancellationToken = default)
    {
        return httpService.PostAsync(
            $"/api/MessageOutbox/{id}/reconsume",
            cancellationToken: cancellationToken
        );
    }
}
