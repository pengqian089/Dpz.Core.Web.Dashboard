using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IInterceptRuleService
{
    Task<IReadOnlyList<InterceptRuleModel>> GetRulesAsync(
        CancellationToken cancellationToken = default
    );

    Task AddAsync(InterceptRuleEditModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(InterceptRuleEditModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
