using System;

namespace Dpz.Core.Web.Dashboard.Models.Request;

/// <summary>
/// 消息 Outbox 查询参数。
/// </summary>
public class MessageOutboxRequest : PaginationRequest
{
    /// <summary>
    /// 关键字，匹配消息 ID、类型、路由、来源和错误信息。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 消息状态。
    /// </summary>
    public OutboxMessageStatus? Status { get; set; }

    /// <summary>
    /// 消息类型。
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// Exchange 名称。
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// 路由键。
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// 消息来源。
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 创建开始时间。
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 创建结束时间。
    /// </summary>
    public DateTime? EndTime { get; set; }
}
