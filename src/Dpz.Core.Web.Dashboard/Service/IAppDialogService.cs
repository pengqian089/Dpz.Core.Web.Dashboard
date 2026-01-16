using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IAppDialogService
{
    Task AlertAsync(string message, string title = "提示");
    Task<bool> ConfirmAsync(string message, string title = "确认");

    Task<string?> PromptAsync(string message, string title = "输入", string defaultValue = "");

    Task<TResult?> ShowComponentAsync<TResult>(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    );

    Task ShowComponentAsync(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    );

    void Toast(string message, ToastType type = ToastType.Info, int duration = 3000);

    // Notification methods
    NotificationModel ShowNotification(
        string content,
        string title = "",
        NotificationType type = NotificationType.Info,
        int autoClose = 0
    );

    NotificationModel ShowNotification(NotificationOptions options);
    void CloseAllNotifications();

    // Events for UI components
    event Action<DialogModel> OnDialogShow;
    event Action<ToastModel> OnToastShow;
    event Action<NotificationModel> OnNotificationShow;
    event Action OnCloseAllNotifications;
}
