using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class BlacklistService(IHttpService httpService) : IBlacklistService
{
    public async Task<IReadOnlyList<BlacklistRecord>> GetBlacklistAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpService.GetAsync<List<BlacklistRecord>>(
            "/api/Blacklist",
            cancellationToken: cancellationToken
        );
        return result ?? [];
    }

    public Task DeleteBlacklistAsync(string id, CancellationToken cancellationToken = default)
    {
        return httpService.DeleteAsync(
            $"/api/Blacklist/{Uri.EscapeDataString(id)}",
            cancellationToken: cancellationToken
        );
    }

    public async Task<IReadOnlyList<BlockedIpInfoModel>> GetBlockedIpsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpService.GetAsync<List<BlockedIpInfoModel>>(
            "/api/Blacklist/blocked-ips",
            cancellationToken: cancellationToken
        );
        return result ?? [];
    }

    public Task BlockIpAsync(
        BlockIpRequestModel request,
        CancellationToken cancellationToken = default
    )
    {
        return httpService.PostAsync("/api/Blacklist/blocked-ips", request, cancellationToken);
    }

    public Task UnblockIpAsync(string ip, CancellationToken cancellationToken = default)
    {
        return httpService.DeleteAsync(
            $"/api/Blacklist/blocked-ips?ip={Uri.EscapeDataString(ip)}",
            cancellationToken: cancellationToken
        );
    }
}
