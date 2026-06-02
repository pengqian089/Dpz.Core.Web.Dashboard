namespace Dpz.Core.Web.Dashboard.Models;

/// <summary>
/// 消息 Outbox 记录的状态枚举。
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>
    /// 待发布：Outbox 记录已创建，消息尚未成功投递到 RabbitMQ。
    /// </summary>
    Pending,

    /// <summary>
    /// 已发布：消息已成功投递到 RabbitMQ，等待被消费者处理。
    /// </summary>
    Sent,

    /// <summary>
    /// 已消费：消息已被消费者成功处理。
    /// </summary>
    Consumed,

    /// <summary>
    /// 发布失败：消息投递到 RabbitMQ 时发生异常，等待后台重试服务
    /// 按指数退避策略重新发布。
    /// </summary>
    PublishFailed,

    /// <summary>
    /// 消费失败：消息经 RabbitMQ 多次重试后仍处理失败，等待后台重试服务
    /// 按指数退避策略重新投递。
    /// </summary>
    ConsumeFailed,
}
