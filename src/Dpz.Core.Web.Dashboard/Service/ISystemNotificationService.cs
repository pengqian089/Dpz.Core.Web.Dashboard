using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Response;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ISystemNotificationService
{
    string HubUrl { get; }

    Task SendAsync(string message, CancellationToken cancellationToken = default);

    Task<List<SystemNotificationHistoryResponse>> GetRecentAsync(
        CancellationToken cancellationToken = default
    );

    Task<IPagedList<SystemNotificationHistoryResponse>> GetPageAsync(
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
