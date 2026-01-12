using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class DialogModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DialogType Type { get; set; }
    public string DefaultValue { get; set; } = "";
    public RenderFragment? Content { get; set; }
    public string Width { get; set; } = "";

    /// <summary>
    /// 是否禁用滚动条
    /// 默认禁用滚动
    /// </summary>
    public bool DisableBodyScroll { get; set; } = true;

    /// <summary>
    /// 是否允许 ESC 关闭
    /// </summary>
    public bool EscToClose { get; set; } = true;

    /// <summary>
    /// 是否允许点击遮罩层关闭
    /// </summary>
    public bool MaskToClose { get; set; } = false;

    public TaskCompletionSource<object?> TaskSource { get; set; } = new();

    /// <summary>
    /// 用于组件反向注册关闭操作，以便外部触发带动画的关闭
    /// </summary>
    public Action? RequestCloseAction { get; set; }
}
