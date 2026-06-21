namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// 通知配置选项
/// </summary>
public class AppNotificationOptions
{
    /// <summary>通知标题</summary>
    public string Title { get; set; } = "";

    /// <summary>通知正文内容</summary>
    public string Content { get; set; } = "";

    /// <summary>进度条数据（可多个条形）</summary>
    public double[] Progress { get; set; } = [];

    /// <summary>反馈级别</summary>
    public AppFeedbackLevel Level { get; set; } = AppFeedbackLevel.Info;

    /// <summary>是否自动关闭</summary>
    public bool AutoClose { get; set; }

    /// <summary>自动关闭前的显示时长（毫秒）</summary>
    public int Duration { get; set; } = 5000;
}
