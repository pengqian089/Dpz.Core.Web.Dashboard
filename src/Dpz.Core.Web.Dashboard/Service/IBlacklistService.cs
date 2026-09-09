using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IBlacklistService
{
    Task<IReadOnlyList<BlacklistRecord>> GetBlacklistAsync(
        CancellationToken cancellationToken = default
    );

    Task DeleteBlacklistAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlockedIpInfoModel>> GetBlockedIpsAsync(
        CancellationToken cancellationToken = default
    );

    Task BlockIpAsync(BlockIpRequestModel request, CancellationToken cancellationToken = default);

    Task UnblockIpAsync(string ip, CancellationToken cancellationToken = default);
}
