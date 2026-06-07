namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// Toast 提示配置选项
/// </summary>
public class AppToastOptions
{
    /// <summary>提示消息内容</summary>
    public string Message { get; set; } = "";
    /// <summary>反馈级别</summary>
    public AppFeedbackLevel Level { get; set; } = AppFeedbackLevel.Info;
    /// <summary>显示时长（毫秒）</summary>
    public int Duration { get; set; } = 3000;
}
