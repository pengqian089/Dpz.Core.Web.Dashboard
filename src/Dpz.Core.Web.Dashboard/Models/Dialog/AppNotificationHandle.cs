using System;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// 通知句柄，承载通知的配置选项与运行时更新回调
/// </summary>
public class AppNotificationHandle
{
    /// <summary>通知唯一标识</summary>
    public string Id { get; } = Guid.NewGuid().ToString();
    /// <summary>通知配置选项</summary>
    public AppNotificationOptions Options { get; set; } = new();

    /// <summary>动态更新正文内容</summary>
    public Action<string>? UpdateContent { get; set; }
    /// <summary>动态更新标题</summary>
    public Action<string>? UpdateTitle { get; set; }
    /// <summary>动态更新进度条数据</summary>
    public Action<double[]>? UpdateProgress { get; set; }
    /// <summary>动态更新反馈级别</summary>
    public Action<AppFeedbackLevel>? UpdateLevel { get; set; }
    /// <summary>关闭通知</summary>
    public Action? Close { get; set; }
}
