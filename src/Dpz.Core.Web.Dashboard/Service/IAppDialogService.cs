using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service;

/// <summary>
/// 应用对话框服务接口，统一管理对话框、Toast 提示和通知的显示与生命周期
/// </summary>
public interface IAppDialogService
{
    /// <summary>显示 Alert 提示框</summary>
    Task ShowAlertAsync(AppDialogOptions options);

    /// <summary>显示 Confirm 确认框，返回用户选择（true=确认，false=取消）</summary>
    Task<bool> ShowConfirmAsync(AppDialogOptions<bool> options);

    /// <summary>显示 Prompt 输入框，返回用户输入值或 null</summary>
    Task<string?> ShowPromptAsync(AppDialogOptions<string?> options);

    /// <summary>显示通用对话框，支持自定义组件和返回值类型</summary>
    Task<TResult?> ShowAsync<TResult>(AppDialogOptions<TResult> options);

    /// <summary>显示 Toast 轻量级提示</summary>
    void ShowToast(AppToastOptions options);

    /// <summary>显示通知，返回可操作的句柄</summary>
    AppNotificationHandle ShowNotification(AppNotificationOptions options);

    /// <summary>关闭所有通知</summary>
    void CloseAllNotifications();

    /// <summary>[兼容旧API] 显示 Alert 提示框</summary>
    Task AlertAsync(string message, string title = "提示");

    /// <summary>[兼容旧API] 显示 Confirm 确认框</summary>
    Task<bool> ConfirmAsync(string message, string title = "确认");

    /// <summary>[兼容旧API] 显示 Prompt 输入框</summary>
    Task<string?> PromptAsync(string message, string title = "输入", string defaultValue = "");

    /// <summary>[兼容旧API] 显示带返回值的组件对话框</summary>
    Task<TResult?> ShowComponentAsync<TResult>(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    );

    /// <summary>[兼容旧API] 显示无返回值的组件对话框</summary>
    Task ShowComponentAsync(
        string title,
        RenderFragment childContent,
        string width = "",
        bool disableBodyScroll = true
    );

    /// <summary>[兼容旧API] 显示 Toast 提示</summary>
    void Toast(string message, ToastType type = ToastType.Info, int duration = 3000);

    /// <summary>[兼容旧API] 显示简单通知</summary>
    NotificationModel ShowNotification(
        string content,
        string title = "",
        NotificationType type = NotificationType.Info,
        int autoClose = 0
    );

    /// <summary>[兼容旧API] 显示带配置的通知</summary>
    NotificationModel ShowNotification(NotificationOptions options);

    /// <summary>对话框显示事件</summary>
    event Action<AppDialogModel> OnDialogShow;

    /// <summary>Toast 显示事件</summary>
    event Action<AppToastModel> OnToastShow;

    /// <summary>通知显示事件</summary>
    event Action<AppNotificationHandle> OnNotificationShow;

    /// <summary>关闭所有通知事件</summary>
    event Action OnCloseAllNotifications;
}
