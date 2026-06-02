using System;
using System.Text.Json.Serialization;

namespace Dpz.Core.Web.Dashboard.Models.Response;

/// <summary>
/// 消息 Outbox 记录响应。
/// </summary>
public class MessageOutboxResponse
{
    /// <summary>
    /// 记录 ID。
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 消息唯一标识。
    /// </summary>
    public required string MessageId { get; set; }

    /// <summary>
    /// 消息类型。
    /// </summary>
    public required string MessageType { get; set; }

    /// <summary>
    /// Exchange 名称。
    /// </summary>
    public required string Exchange { get; set; }

    /// <summary>
    /// 路由键。
    /// </summary>
    public required string RoutingKey { get; set; }

    /// <summary>
    /// JSON 序列化的消息体。
    /// </summary>
    public required string Payload { get; set; }

    /// <summary>
    /// 消息来源。
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 当前状态。
    /// </summary>
    [JsonConverter(typeof(EnumConverter<OutboxMessageStatus>))]
    public OutboxMessageStatus Status { get; set; }

    /// <summary>
    /// 发布尝试次数。
    /// </summary>
    public int PublishAttempts { get; set; }

    /// <summary>
    /// 最后一次发布尝试时间。
    /// </summary>
    public DateTime? LastPublishAttemptAt { get; set; }

    /// <summary>
    /// 下次发布重试时间。
    /// </summary>
    public DateTime? NextPublishRetryAt { get; set; }

    /// <summary>
    /// 发布时间。
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// 最后一次发布失败信息。
    /// </summary>
    public string? LastPublishError { get; set; }

    /// <summary>
    /// 消费尝试次数。
    /// </summary>
    public int ConsumeAttempts { get; set; }

    /// <summary>
    /// 最后一次消费尝试时间。
    /// </summary>
    public DateTime? LastConsumeAttemptAt { get; set; }

    /// <summary>
    /// 下次消费重试时间。
    /// </summary>
    public DateTime? NextConsumeRetryAt { get; set; }

    /// <summary>
    /// 消费时间。
    /// </summary>
    public DateTime? ConsumedAt { get; set; }

    /// <summary>
    /// 最后一次消费失败信息。
    /// </summary>
    public string? LastConsumeError { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}
