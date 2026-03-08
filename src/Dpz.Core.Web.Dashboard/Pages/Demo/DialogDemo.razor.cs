using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Demo;

public partial class DialogDemo(IAppDialogService dialogService)
{
    private string _lastResult = "";

    private async Task ShowAlert()
    {
        await dialogService.AlertAsync("这是一个简单的提示框", "提示");
        _lastResult = "Alert: 用户点击了确定";
    }

    private async Task ShowConfirm()
    {
        var confirmed = await dialogService.ConfirmAsync(
            "确定要执行此操作吗？此操作不可撤销。",
            "确认操作"
        );
        _lastResult = confirmed ? "Confirm: 用户点击了确定" : "Confirm: 用户点击了取消";
    }

    private async Task ShowPrompt()
    {
        var result = await dialogService.PromptAsync(
            "请输入您的名字：",
            "输入信息",
            "张三"
        );
        _lastResult = result != null ? $"Prompt: 用户输入了 '{result}'" : "Prompt: 用户取消了输入";
    }

    private async Task ShowCustomDialog()
    {
        var content = CreateCustomContent();
        await dialogService.ShowComponentAsync("自定义内容", content, "600px");
        _lastResult = "Custom Dialog: 对话框已关闭";
    }

    private RenderFragment CreateCustomContent() =>
        builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "style", "padding: 20px;");

            builder.OpenElement(2, "p");
            builder.AddContent(
                3,
                "这是一个自定义对话框，可以包含任意复杂的内容和交互。"
            );
            builder.CloseElement();

            builder.OpenElement(4, "ul");
            builder.AddAttribute(5, "style", "margin: 20px 0;");
            for (var i = 1; i <= 5; i++)
            {
                builder.OpenElement(6, "li");
                builder.AddContent(7, $"列表项 {i}");
                builder.CloseElement();
            }
            builder.CloseElement();

            builder.OpenElement(8, "p");
            builder.AddAttribute(9, "style", "color: var(--text-secondary);");
            builder.AddContent(10, "可以包含任何 Blazor 组件或 HTML 内容。");
            builder.CloseElement();

            builder.CloseElement();
        };

    private void ShowToast(ToastType type)
    {
        var messages = new System.Collections.Generic.Dictionary<ToastType, string>
        {
            { ToastType.Success, "操作成功！" },
            { ToastType.Error, "操作失败，请重试" },
            { ToastType.Info, "这是一条提示信息" },
            { ToastType.Warning, "警告：请注意检查" },
        };

        dialogService.Toast(messages[type], type);
        _lastResult = $"Toast: 显示了 {type} 类型的提示";
    }

    private void ShowNotification(NotificationType type)
    {
        var options = new NotificationOptions
        {
            Type = type,
            Title = GetNotificationTitle(type),
            Content = GetNotificationContent(type),
            AutoClose = 5000,
        };

        dialogService.ShowNotification(options);
        _lastResult = $"Notification: 显示了 {type} 类型的通知";
    }

    private void ShowProgressNotification()
    {
        var options = new NotificationOptions
        {
            Type = NotificationType.Info,
            Title = "任务进度",
            Content = "正在处理您的请求...",
            Bars = [65.5, 82.3],
            AutoClose = 0,
        };

        dialogService.ShowNotification(options);
        _lastResult = "Notification: 显示了带进度条的通知";
    }

    private void CloseAllNotifications()
    {
        dialogService.CloseAllNotifications();
        _lastResult = "Notification: 已清除所有通知";
    }

    private static string GetNotificationTitle(NotificationType type) =>
        type switch
        {
            NotificationType.Success => "操作成功",
            NotificationType.Error => "操作失败",
            NotificationType.Info => "系统消息",
            NotificationType.Warning => "警告提示",
            _ => "通知",
        };

    private static string GetNotificationContent(NotificationType type) =>
        type switch
        {
            NotificationType.Success => "您的操作已成功完成，数据已保存。",
            NotificationType.Error => "操作过程中发生错误，请稍后重试。",
            NotificationType.Info => "系统将在 5 分钟后进行维护，请提前保存工作。",
            NotificationType.Warning => "检测到异常访问行为，请确认是否为本人操作。",
            _ => "这是一条通知消息",
        };
}
