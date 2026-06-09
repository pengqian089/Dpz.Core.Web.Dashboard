namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// 对话框类型枚举
/// </summary>
public enum AppDialogType
{
    /// <summary>提示框</summary>
    Alert,

    /// <summary>确认框</summary>
    Confirm,

    /// <summary>输入框</summary>
    Prompt,

    /// <summary>自定义组件</summary>
    Component,
}
