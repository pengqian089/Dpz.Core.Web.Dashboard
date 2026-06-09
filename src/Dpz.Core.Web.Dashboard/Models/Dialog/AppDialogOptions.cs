using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// 对话框配置选项（非泛型基类，用于 Alert 等无返回值的对话框）
/// </summary>
public class AppDialogOptions
{
    /// <summary>标题</summary>
    public string Title { get; set; } = "提示";

    /// <summary>正文消息</summary>
    public string Message { get; set; } = "";

    /// <summary>自定义宽度（CSS 值）</summary>
    public string Width { get; set; } = "";

    /// <summary>点击遮罩层是否关闭</summary>
    public bool LightDismiss { get; set; }

    /// <summary>按 ESC 是否关闭</summary>
    public bool EscToClose { get; set; } = true;

    /// <summary>确定按钮文本</summary>
    public string ConfirmText { get; set; } = "确定";

    /// <summary>取消按钮文本</summary>
    public string CancelText { get; set; } = "取消";
}

/// <summary>
/// 泛型对话框配置选项，支持带返回值的对话框（Confirm / Prompt / Component）
/// </summary>
/// <typeparam name="TResult">对话框关闭时的返回值类型</typeparam>
public class AppDialogOptions<TResult> : AppDialogOptions
{
    /// <summary>对话框类型</summary>
    public AppDialogType Type { get; set; } = AppDialogType.Component;

    /// <summary>Prompt 类型的默认输入值</summary>
    public string DefaultValue { get; set; } = "";

    /// <summary>组件类型的自定义内容</summary>
    public RenderFragment? Content { get; set; }

    /// <summary>用于等待关闭结果的 TaskCompletionSource</summary>
    public TaskCompletionSource<TResult?> TaskSource { get; set; } = new();
}
