using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class AppDialogService : IAppDialogService
{
    public event Action<DialogModel>? OnDialogShow;
    public event Action<ToastModel>? OnToastShow;
    public event Action<NotificationModel>? OnNotificationShow;
    public event Action? OnCloseAllNotifications;

    public async Task AlertAsync(string message, string title = "提示")
    {
        var tcs = new TaskCompletionSource<object?>();
        var model = new DialogModel
        {
            Title = title,
            Message = message,
            Type = DialogType.Alert,
            TaskSource = tcs,
        };

        OnDialogShow?.Invoke(model);
        await tcs.Task;
    }

    public async Task<bool> ConfirmAsync(string message, string title = "确认")
    {
        var tcs = new TaskCompletionSource<object?>();
        var model = new DialogModel
        {
            Title = title,
            Message = message,
            Type = DialogType.Confirm,
            TaskSource = tcs,
        };

        OnDialogShow?.Invoke(model);
        var result = await tcs.Task;
        return result is true;
    }

    public async Task<string?> PromptAsync(
        string message,
        string title = "输入",
        string defaultValue = ""
    )
    {
        var tcs = new TaskCompletionSource<object?>();
        var model = new DialogModel
        {
            Title = title,
            Message = message,
            Type = DialogType.Prompt,
            DefaultValue = defaultValue,
            TaskSource = tcs,
        };

        OnDialogShow?.Invoke(model);
        var result = await tcs.Task;
        return result as string;
    }

    public Task<TResult?> ShowComponentAsync<TResult>(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    )
    {
        var tcs = new TaskCompletionSource<object?>();
        var model = new DialogModel
        {
            Title = title,
            Type = DialogType.Component,
            Content = childContent,
            Width = width,
            DisableBodyScroll = disableBodyScroll,
            TaskSource = tcs,
        };

        OnDialogShow?.Invoke(model);
        // Cast result to TResult
        return tcs.Task.ContinueWith(t => t.Result is TResult r ? r : default);
    }

    public async Task ShowComponentAsync(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    )
    {
        var tcs = new TaskCompletionSource<object?>();
        var model = new DialogModel
        {
            Title = title,
            Type = DialogType.Component,
            Content = childContent,
            Width = width,
            DisableBodyScroll = disableBodyScroll,
            TaskSource = tcs,
        };

        OnDialogShow?.Invoke(model);
        await tcs.Task;
    }

    public void Toast(string message, ToastType type = ToastType.Info, int duration = 3000)
    {
        var model = new ToastModel
        {
            Message = message,
            Type = type,
            Duration = duration,
        };
        OnToastShow?.Invoke(model);
    }

    public NotificationModel ShowNotification(
        string content,
        string title = "",
        NotificationType type = NotificationType.Info,
        int autoClose = 0
    )
    {
        return ShowNotification(
            new NotificationOptions
            {
                Content = content,
                Title = title,
                Type = type,
                AutoClose = autoClose,
            }
        );
    }

    public NotificationModel ShowNotification(NotificationOptions options)
    {
        var model = new NotificationModel { Options = options };
        OnNotificationShow?.Invoke(model);
        return model;
    }

    public void CloseAllNotifications()
    {
        OnCloseAllNotifications?.Invoke();
    }
}
