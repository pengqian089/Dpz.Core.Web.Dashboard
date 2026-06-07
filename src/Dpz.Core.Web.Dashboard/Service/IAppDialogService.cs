using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IAppDialogService
{
    Task ShowAlertAsync(AppDialogOptions options);
    Task<bool> ShowConfirmAsync(AppDialogOptions<bool> options);
    Task<string?> ShowPromptAsync(AppDialogOptions<string?> options);
    Task<TResult?> ShowAsync<TResult>(AppDialogOptions<TResult> options);
    void ShowToast(AppToastOptions options);
    AppNotificationHandle ShowNotification(AppNotificationOptions options);
    void CloseAllNotifications();

    // Compatibility wrappers for existing call sites.
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

    NotificationModel ShowNotification(
        string content,
        string title = "",
        NotificationType type = NotificationType.Info,
        int autoClose = 0
    );

    NotificationModel ShowNotification(NotificationOptions options);

    event Action<AppDialogModel> OnDialogShow;
    event Action<AppToastModel> OnToastShow;
    event Action<AppNotificationHandle> OnNotificationShow;
    event Action OnCloseAllNotifications;
}
