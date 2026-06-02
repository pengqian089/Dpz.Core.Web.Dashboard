using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IMessageOutboxService
{
    Task<IPagedList<MessageOutboxResponse>> GetPageAsync(
        MessageOutboxRequest request,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );

    Task<MessageOutboxFilterOptionsResponse?> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
