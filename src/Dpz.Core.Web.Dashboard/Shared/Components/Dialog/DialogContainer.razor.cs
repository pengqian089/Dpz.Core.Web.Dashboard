using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components.Dialog;

public partial class DialogContainer(IAppDialogService dialogService, IJSRuntime jsRuntime)
    : IAsyncDisposable
{
    private readonly List<DialogModel> _dialogs = [];
    private readonly List<ToastModel> _toasts = [];
    private readonly List<NotificationModel> _notifications = [];
    private IJSObjectReference? _dialogModule;

    protected override void OnInitialized()
    {
        dialogService.OnDialogShow += ShowDialog;
        dialogService.OnToastShow += ShowToast;
        dialogService.OnNotificationShow += ShowNotification;
        dialogService.OnCloseAllNotifications += CloseAllNotifications;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dialogModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./js/modules/dialog-interop.js"
            );

            // 初始化全局按键监听
            if (_dialogModule != null)
            {
                var dotNetHelper = DotNetObjectReference.Create(this);
                await _dialogModule.InvokeVoidAsync("initKeyboardListener", dotNetHelper);
            }
        }
    }

    [JSInvokable]
    public async Task HandleGlobalEsc()
    {
        // 查找最后一个（最上层）可被 Esc 关闭的对话框
        var dialog = _dialogs.LastOrDefault();

        if (dialog != null && dialog.EscToClose)
        {
            if (dialog.RequestCloseAction != null)
            {
                dialog.RequestCloseAction.Invoke();
            }
            else
            {
                // 如果没有注册 Action (比如还没渲染完成)，直接移除
                RemoveDialog(dialog);
                dialog.TaskSource.TrySetResult(null);
            }
        }
        await Task.CompletedTask;
    }

    private async void ShowDialog(DialogModel model)
    {
        _dialogs.Add(model);
        await InvokeAsync(StateHasChanged);

        if (_dialogModule != null)
        {
            try
            {
                await _dialogModule.InvokeVoidAsync("disableBodyScroll", model.DisableBodyScroll);
            }
            catch
            {
                // Fallback if module not loaded
            }
        }
    }

    private async void RemoveDialog(DialogModel model)
    {
        _dialogs.Remove(model);
        await InvokeAsync(StateHasChanged);

        if (_dialogs.Count == 0 && _dialogModule != null)
        {
            try
            {
                await _dialogModule.InvokeVoidAsync("enableBodyScroll");
            }
            catch
            {
                // Ignore if module disposed
            }
        }
    }

    private void ShowToast(ToastModel model)
    {
        _toasts.Add(model);
        InvokeAsync(StateHasChanged);
    }

    private void RemoveToast(ToastModel model)
    {
        _toasts.Remove(model);
        InvokeAsync(StateHasChanged);
    }

    private void ShowNotification(NotificationModel model)
    {
        _notifications.Add(model);
        InvokeAsync(StateHasChanged);
    }

    private void RemoveNotification(NotificationModel model)
    {
        _notifications.Remove(model);
        InvokeAsync(StateHasChanged);
    }

    private void CloseAllNotifications()
    {
        // This is tricky because we want animation.
        // We can call Close on all models.
        foreach (var n in _notifications.ToList())
        {
            n.Close?.Invoke();
        }
    }

    public async ValueTask DisposeAsync()
    {
        dialogService.OnDialogShow -= ShowDialog;
        dialogService.OnToastShow -= ShowToast;
        dialogService.OnNotificationShow -= ShowNotification;
        dialogService.OnCloseAllNotifications -= CloseAllNotifications;

        if (_dialogModule != null)
        {
            try
            {
                await _dialogModule.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
    }
}
