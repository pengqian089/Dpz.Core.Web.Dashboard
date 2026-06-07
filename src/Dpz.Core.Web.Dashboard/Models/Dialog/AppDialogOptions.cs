using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class AppDialogOptions
{
    public string Title { get; set; } = "提示";
    public string Message { get; set; } = "";
    public string Width { get; set; } = "";
    public bool LightDismiss { get; set; }
    public bool EscToClose { get; set; } = true;
    public string ConfirmText { get; set; } = "确定";
    public string CancelText { get; set; } = "取消";
}

public class AppDialogOptions<TResult> : AppDialogOptions
{
    public AppDialogType Type { get; set; } = AppDialogType.Component;
    public string DefaultValue { get; set; } = "";
    public RenderFragment? Content { get; set; }
    public TaskCompletionSource<TResult?> TaskSource { get; set; } = new();
}
