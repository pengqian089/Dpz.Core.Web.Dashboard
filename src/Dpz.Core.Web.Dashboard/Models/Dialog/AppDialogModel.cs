using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

/// <summary>
/// 对话框内部模型，承载对话框的完整状态与交互契约
/// </summary>
public class AppDialogModel
{
    /// <summary>对话框唯一标识</summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>对话框类型</summary>
    public AppDialogType Type { get; set; }

    /// <summary>标题</summary>
    public string Title { get; set; } = "";

    /// <summary>正文消息</summary>
    public string Message { get; set; } = "";

    /// <summary>自定义宽度（CSS 值）</summary>
    public string Width { get; set; } = "";

    /// <summary>Prompt 类型的默认输入值</summary>
    public string DefaultValue { get; set; } = "";

    /// <summary>点击遮罩层是否关闭</summary>
    public bool LightDismiss { get; set; }

    /// <summary>按 ESC 是否关闭</summary>
    public bool EscToClose { get; set; } = true;

    /// <summary>确定按钮文本</summary>
    public string ConfirmText { get; set; } = "确定";

    /// <summary>取消按钮文本</summary>
    public string CancelText { get; set; } = "取消";

    /// <summary>组件类型的自定义内容</summary>
    public RenderFragment? Content { get; set; }

    /// <summary>用于等待关闭结果的 TaskCompletionSource</summary>
    public TaskCompletionSource<object?> TaskSource { get; set; } = new();

    /// <summary>外部请求关闭对话框的回调</summary>
    public Action<object?>? RequestClose { get; set; }
}
