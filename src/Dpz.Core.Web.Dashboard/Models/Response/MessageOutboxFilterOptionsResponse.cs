using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models.Response;

/// <summary>
/// 消息 Outbox 筛选项响应。
/// </summary>
public class MessageOutboxFilterOptionsResponse
{
    /// <summary>
    /// 消息类型列表。
    /// </summary>
    public List<string> MessageTypes { get; set; } = [];

    /// <summary>
    /// Exchange 列表。
    /// </summary>
    public List<string> Exchanges { get; set; } = [];

    /// <summary>
    /// 路由键列表。
    /// </summary>
    public List<string> RoutingKeys { get; set; } = [];

    /// <summary>
    /// 消息来源列表。
    /// </summary>
    public List<string> Sources { get; set; } = [];
}
