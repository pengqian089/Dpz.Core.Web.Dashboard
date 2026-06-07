using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components.Dialog;

public partial class DialogContainer(
    IAppDialogService dialogService,
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService
) : IAsyncDisposable
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
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/interop/dialog-interop.ts"
            );
            _dialogModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);

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
        var dialog = _dialogs.LastOrDefault();

        if (dialog != null && dialog.EscToClose)
        {
            if (dialog.RequestCloseAction != null)
            {
                dialog.RequestCloseAction.Invoke();
            }
            else
            {
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
                Console.WriteLine("Failed to disable body scroll.");
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
                Console.WriteLine("Failed to enable body scroll.");
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
                Console.WriteLine("Failed to dispose dialog module.");
            }
        }
    }
}
