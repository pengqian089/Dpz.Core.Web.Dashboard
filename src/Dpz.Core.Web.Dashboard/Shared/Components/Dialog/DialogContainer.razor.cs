using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;

namespace Dpz.Core.Web.Dashboard.Shared.Components.Dialog;

public partial class DialogContainer(IAppDialogService dialogService) : IDisposable
{
    private readonly List<AppDialogModel> _dialogs = [];
    private readonly List<AppToastModel> _toasts = [];
    private readonly List<AppNotificationHandle> _notifications = [];
    private readonly Dictionary<string, Timer> _toastTimers = [];
    private readonly Dictionary<string, Timer> _notificationTimers = [];

    protected override void OnInitialized()
    {
        dialogService.OnDialogShow += ShowDialog;
        dialogService.OnToastShow += ShowToast;
        dialogService.OnNotificationShow += ShowNotification;
        dialogService.OnCloseAllNotifications += CloseAllNotifications;
    }

    private void ShowDialog(AppDialogModel model)
    {
        _dialogs.Add(model);
        InvokeAsync(StateHasChanged);
    }

    private void RemoveDialog(AppDialogModel model)
    {
        _dialogs.Remove(model);
        InvokeAsync(StateHasChanged);
    }

    private void ShowToast(AppToastModel model)
    {
        _toasts.Add(model);
        _toastTimers[model.Id] = new Timer(
            _ => InvokeAsync(() => RemoveToast(model)),
            null,
            Math.Max(0, model.Options.Duration),
            Timeout.Infinite
        );

        InvokeAsync(StateHasChanged);
    }

    private void RemoveToast(AppToastModel model)
    {
        _toasts.Remove(model);
        DisposeTimer(_toastTimers, model.Id);
        StateHasChanged();
    }

    private void ShowNotification(AppNotificationHandle handle)
    {
        handle.UpdateContent = content =>
        {
            handle.Options.Content = content;
            InvokeAsync(StateHasChanged);
        };
        handle.UpdateTitle = title =>
        {
            handle.Options.Title = title;
            InvokeAsync(StateHasChanged);
        };
        handle.UpdateProgress = progress =>
        {
            handle.Options.Progress = progress;
            InvokeAsync(StateHasChanged);
        };
        handle.UpdateLevel = level =>
        {
            handle.Options.Level = level;
            InvokeAsync(StateHasChanged);
        };
        handle.Close = () => InvokeAsync(() => CloseNotification(handle));

        _notifications.Add(handle);

        if (handle.Options.AutoClose)
        {
            _notificationTimers[handle.Id] = new Timer(
                _ => InvokeAsync(() => CloseNotification(handle)),
                null,
                Math.Max(0, handle.Options.Duration),
                Timeout.Infinite
            );
        }

        InvokeAsync(StateHasChanged);
    }

    private void CloseNotification(AppNotificationHandle handle)
    {
        _notifications.Remove(handle);
        DisposeTimer(_notificationTimers, handle.Id);
        ClearNotificationCallbacks(handle);
        StateHasChanged();
    }

    private void CloseAllNotifications()
    {
        foreach (var notification in _notifications.ToList())
        {
            CloseNotification(notification);
        }
    }

    private static string GetVariant(AppFeedbackLevel level)
    {
        return level switch
        {
            AppFeedbackLevel.Success => "success",
            AppFeedbackLevel.Warning => "warning",
            AppFeedbackLevel.Danger => "danger",
            _ => "brand",
        };
    }

    private static string GetIconClass(AppFeedbackLevel level)
    {
        return level switch
        {
            AppFeedbackLevel.Success => "fas fa-check-circle",
            AppFeedbackLevel.Warning => "fas fa-exclamation-triangle",
            AppFeedbackLevel.Danger => "fas fa-times-circle",
            _ => "fas fa-info-circle",
        };
    }

    private static void DisposeTimer(Dictionary<string, Timer> timers, string id)
    {
        if (timers.Remove(id, out var timer))
        {
            timer.Dispose();
        }
    }

    private static void ClearNotificationCallbacks(AppNotificationHandle handle)
    {
        handle.UpdateContent = null;
        handle.UpdateTitle = null;
        handle.UpdateProgress = null;
        handle.UpdateLevel = null;
        handle.Close = null;
    }

    public void Dispose()
    {
        dialogService.OnDialogShow -= ShowDialog;
        dialogService.OnToastShow -= ShowToast;
        dialogService.OnNotificationShow -= ShowNotification;
        dialogService.OnCloseAllNotifications -= CloseAllNotifications;

        foreach (var timer in _toastTimers.Values.Concat(_notificationTimers.Values))
        {
            timer.Dispose();
        }

        foreach (var notification in _notifications)
        {
            ClearNotificationCallbacks(notification);
        }
    }
}
