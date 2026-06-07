using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class AppDialogService : IAppDialogService
{
    public event Action<AppDialogModel>? OnDialogShow;
    public event Action<AppToastModel>? OnToastShow;
    public event Action<AppNotificationHandle>? OnNotificationShow;
    public event Action? OnCloseAllNotifications;

    public Task ShowAlertAsync(AppDialogOptions options)
    {
        var dialog = CreateDialog<object?>(
            new AppDialogOptions<object?>
            {
                Type = AppDialogType.Alert,
                Title = options.Title,
                Message = options.Message,
                Width = options.Width,
                LightDismiss = options.LightDismiss,
                EscToClose = options.EscToClose,
                ConfirmText = options.ConfirmText,
                CancelText = options.CancelText,
            }
        );

        OnDialogShow?.Invoke(dialog);
        return dialog.TaskSource.Task;
    }

    public Task<bool> ShowConfirmAsync(AppDialogOptions<bool> options)
    {
        options.Type = AppDialogType.Confirm;
        var dialog = CreateDialog(options);
        OnDialogShow?.Invoke(dialog);
        return CompleteTypedAsync<bool>(dialog.TaskSource.Task);
    }

    public Task<string?> ShowPromptAsync(AppDialogOptions<string?> options)
    {
        options.Type = AppDialogType.Prompt;
        var dialog = CreateDialog(options);
        OnDialogShow?.Invoke(dialog);
        return CompleteTypedAsync<string?>(dialog.TaskSource.Task);
    }

    public Task<TResult?> ShowAsync<TResult>(AppDialogOptions<TResult> options)
    {
        var dialog = CreateDialog(options);
        OnDialogShow?.Invoke(dialog);
        return CompleteTypedAsync<TResult>(dialog.TaskSource.Task);
    }

    public void ShowToast(AppToastOptions options)
    {
        OnToastShow?.Invoke(new AppToastModel { Options = options });
    }

    public AppNotificationHandle ShowNotification(AppNotificationOptions options)
    {
        var handle = new AppNotificationHandle { Options = options };
        OnNotificationShow?.Invoke(handle);
        return handle;
    }

    public void CloseAllNotifications()
    {
        OnCloseAllNotifications?.Invoke();
    }

    public Task AlertAsync(string message, string title = "提示")
    {
        return ShowAlertAsync(new AppDialogOptions { Title = title, Message = message });
    }

    public Task<bool> ConfirmAsync(string message, string title = "确认")
    {
        return ShowConfirmAsync(
            new AppDialogOptions<bool>
            {
                Title = title,
                Message = message,
                Type = AppDialogType.Confirm,
            }
        );
    }

    public Task<string?> PromptAsync(
        string message,
        string title = "输入",
        string defaultValue = ""
    )
    {
        return ShowPromptAsync(
            new AppDialogOptions<string?>
            {
                Title = title,
                Message = message,
                DefaultValue = defaultValue,
                Type = AppDialogType.Prompt,
            }
        );
    }

    public Task<TResult?> ShowComponentAsync<TResult>(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    )
    {
        return ShowAsync(
            new AppDialogOptions<TResult>
            {
                Title = title,
                Content = childContent,
                Width = width,
                Type = AppDialogType.Component,
            }
        );
    }

    public async Task ShowComponentAsync(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    )
    {
        await ShowAsync<object?>(
            new AppDialogOptions<object?>
            {
                Title = title,
                Content = childContent,
                Width = width,
                Type = AppDialogType.Component,
            }
        );
    }

    public void Toast(string message, ToastType type = ToastType.Info, int duration = 3000)
    {
        ShowToast(
            new AppToastOptions
            {
                Message = message,
                Level = MapToastType(type),
                Duration = duration,
            }
        );
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
        var handle = ShowNotification(
            new AppNotificationOptions
            {
                Title = options.Title,
                Content = options.Content,
                Progress = options.Bars,
                Level = MapNotificationType(options.Type),
                AutoClose = options.AutoClose > 0,
                Duration = options.AutoClose > 0 ? options.AutoClose : 5000,
            }
        );

        return new NotificationModel
        {
            Id = handle.Id,
            Options = options,
            UpdateContent = content => handle.UpdateContent?.Invoke(content),
            UpdateTitle = title => handle.UpdateTitle?.Invoke(title),
            UpdateProgress = bars => handle.UpdateProgress?.Invoke(bars),
            UpdateType = type => handle.UpdateLevel?.Invoke(MapNotificationType(type)),
            Close = () => handle.Close?.Invoke(),
        };
    }

    private static AppDialogModel CreateDialog<TResult>(AppDialogOptions<TResult> options)
    {
        return new AppDialogModel
        {
            Type = options.Type,
            Title = options.Title,
            Message = options.Message,
            Width = options.Width,
            LightDismiss = options.LightDismiss,
            EscToClose = options.EscToClose,
            ConfirmText = options.ConfirmText,
            CancelText = options.CancelText,
            DefaultValue = options.DefaultValue,
            Content = options.Content,
            TaskSource = new TaskCompletionSource<object?>(),
        };
    }

    private static async Task<TResult?> CompleteTypedAsync<TResult>(Task<object?> task)
    {
        var result = await task;
        return result is TResult typed ? typed : default;
    }

    private static AppFeedbackLevel MapToastType(ToastType type)
    {
        return type switch
        {
            ToastType.Success => AppFeedbackLevel.Success,
            ToastType.Warning => AppFeedbackLevel.Warning,
            ToastType.Error => AppFeedbackLevel.Danger,
            _ => AppFeedbackLevel.Info,
        };
    }

    private static AppFeedbackLevel MapNotificationType(NotificationType type)
    {
        return type switch
        {
            NotificationType.Success => AppFeedbackLevel.Success,
            NotificationType.Warning => AppFeedbackLevel.Warning,
            NotificationType.Error => AppFeedbackLevel.Danger,
            _ => AppFeedbackLevel.Info,
        };
    }
}
