using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class AppDialogModel
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public AppDialogType Type { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Width { get; set; } = "";
    public string DefaultValue { get; set; } = "";
    public bool LightDismiss { get; set; }
    public bool EscToClose { get; set; } = true;
    public string ConfirmText { get; set; } = "确定";
    public string CancelText { get; set; } = "取消";
    public RenderFragment? Content { get; set; }
    public TaskCompletionSource<object?> TaskSource { get; set; } = new();
    public Action<object?>? RequestClose { get; set; }
}
