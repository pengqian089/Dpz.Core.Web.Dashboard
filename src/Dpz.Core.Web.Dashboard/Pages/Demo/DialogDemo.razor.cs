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
        await dialogService.ShowAlertAsync(
            new AppDialogOptions { Title = "提示", Message = "这是一个简单的提示框" }
        );
        _lastResult = "Alert: 用户点击了确定";
    }

    private async Task ShowConfirm()
    {
        var confirmed = await dialogService.ShowConfirmAsync(
            new AppDialogOptions<bool>
            {
                Title = "确认操作",
                Message = "确定要执行此操作吗？此操作不可撤销。",
            }
        );
        _lastResult = confirmed ? "Confirm: 用户点击了确定" : "Confirm: 用户点击了取消";
    }

    private async Task ShowPrompt()
    {
        var result = await dialogService.ShowPromptAsync(
            new AppDialogOptions<string?>
            {
                Title = "输入信息",
                Message = "请输入您的名字：",
                DefaultValue = "张三",
            }
        );
        _lastResult = result != null ? $"Prompt: 用户输入了 '{result}'" : "Prompt: 用户取消了输入";
    }

    private async Task ShowCustomDialog()
    {
        var content = CreateCustomContent();
        await dialogService.ShowAsync<object?>(
            new AppDialogOptions<object?>
            {
                Title = "自定义内容",
                Content = content,
                Width = "600px",
                Type = AppDialogType.Component,
            }
        );
        _lastResult = "Custom Dialog: 对话框已关闭";
    }

    private RenderFragment CreateCustomContent() =>
        builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "style", "padding: 20px;");

            builder.OpenElement(2, "p");
            builder.AddContent(3, "这是一个自定义对话框，可以包含任意复杂的内容和交互。");
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

    private void ShowToast(AppFeedbackLevel level)
    {
        var messages = new System.Collections.Generic.Dictionary<AppFeedbackLevel, string>
        {
            { AppFeedbackLevel.Success, "操作成功！" },
            { AppFeedbackLevel.Danger, "操作失败，请重试" },
            { AppFeedbackLevel.Info, "这是一条提示信息" },
            { AppFeedbackLevel.Warning, "警告：请注意检查" },
        };

        dialogService.ShowToast(new AppToastOptions { Message = messages[level], Level = level });
        _lastResult = $"Toast: 显示了 {level} 类型的提示";
    }

    private void ShowNotification(AppFeedbackLevel level)
    {
        var options = new AppNotificationOptions
        {
            Level = level,
            Title = GetNotificationTitle(level),
            Content = GetNotificationContent(level),
            AutoClose = true,
            Duration = 5000,
        };

        dialogService.ShowNotification(options);
        _lastResult = $"Notification: 显示了 {level} 类型的通知";
    }

    private void ShowProgressNotification()
    {
        var options = new AppNotificationOptions
        {
            Level = AppFeedbackLevel.Info,
            Title = "任务进度",
            Content = "正在处理您的请求...",
            Progress = [65.5, 82.3],
        };

        dialogService.ShowNotification(options);
        _lastResult = "Notification: 显示了带进度条的通知";
    }

    private void CloseAllNotifications()
    {
        dialogService.CloseAllNotifications();
        _lastResult = "Notification: 已清除所有通知";
    }

    private static string GetNotificationTitle(AppFeedbackLevel level) =>
        level switch
        {
            AppFeedbackLevel.Success => "操作成功",
            AppFeedbackLevel.Danger => "操作失败",
            AppFeedbackLevel.Info => "系统消息",
            AppFeedbackLevel.Warning => "警告提示",
            _ => "通知",
        };

    private static string GetNotificationContent(AppFeedbackLevel level) =>
        level switch
        {
            AppFeedbackLevel.Success => "您的操作已成功完成，数据已保存。",
            AppFeedbackLevel.Danger => "操作过程中发生错误，请稍后重试。",
            AppFeedbackLevel.Info => "系统将在 5 分钟后进行维护，请提前保存工作。",
            AppFeedbackLevel.Warning => "检测到异常访问行为，请确认是否为本人操作。",
            _ => "这是一条通知消息",
        };
}
