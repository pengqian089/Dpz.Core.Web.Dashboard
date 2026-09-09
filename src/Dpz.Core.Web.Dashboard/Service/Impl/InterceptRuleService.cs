using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class InterceptRuleService(IHttpService httpService) : IInterceptRuleService
{
    public async Task<IReadOnlyList<InterceptRuleModel>> GetRulesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpService.GetAsync<List<InterceptRuleModel>>(
            "/api/InterceptRule",
            cancellationToken: cancellationToken
        );
        return result ?? [];
    }

    public Task AddAsync(
        InterceptRuleEditModel model,
        CancellationToken cancellationToken = default
    )
    {
        return httpService.PostAsync(
            "/api/InterceptRule",
            new
            {
                model.Type,
                model.Pattern,
                model.Key,
            },
            cancellationToken
        );
    }

    public Task UpdateAsync(
        InterceptRuleEditModel model,
        CancellationToken cancellationToken = default
    )
    {
        return httpService.PutAsync(
            "/api/InterceptRule",
            new
            {
                Id = model.Id,
                model.Type,
                model.Pattern,
                model.Key,
            },
            cancellationToken
        );
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return httpService.DeleteAsync(
            $"/api/InterceptRule/{Uri.EscapeDataString(id)}",
            cancellationToken: cancellationToken
        );
    }
}
