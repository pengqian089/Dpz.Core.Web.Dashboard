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

    /// <summary>
    /// 删除 Outbox 记录。
    /// </summary>
    /// <param name="id">记录 ID。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 手动将已发布但未消费成功的 Outbox 消息重新入队。
    /// </summary>
    /// <param name="id">记录 ID。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task ReconsumeAsync(string id, CancellationToken cancellationToken = default);
}
