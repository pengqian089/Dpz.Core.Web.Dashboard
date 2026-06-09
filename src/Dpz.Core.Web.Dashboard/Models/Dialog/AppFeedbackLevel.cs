namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// 反馈级别枚举（用于 Toast 和 Notification）
/// </summary>
public enum AppFeedbackLevel
{
    /// <summary>信息</summary>
    Info,

    /// <summary>成功</summary>
    Success,

    /// <summary>警告</summary>
    Warning,

    /// <summary>危险/错误</summary>
    Danger,
}
