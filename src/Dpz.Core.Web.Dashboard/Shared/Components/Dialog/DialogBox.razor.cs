using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components.Dialog;

/// <summary>
/// 对话框 Popup 组件，使用 Web Awesome 组件库实现动画、焦点管理和键盘交互
/// </summary>
public partial class DialogBox(IJSRuntime jsRuntime, IAssetManifestService assetManifestService)
    : IAsyncDisposable
{
    /// <summary>
    /// 对话框的数据模型，包含类型、标题、内容及等待关闭的 TaskCompletionSource
    /// </summary>
    [Parameter]
    public AppDialogModel Model { get; set; } = new();

    /// <summary>
    /// 对话框关闭时的回调事件
    /// </summary>
    [Parameter]
    public EventCallback<AppDialogModel> OnClose { get; set; }

    private string _inputValue = "";
    private bool _isClosing;
    private ElementReference _dialogRef;
    private ElementReference _inputRef;
    private IJSObjectReference? _dialogModule;
    private DotNetObjectReference<DialogBox>? _dotNetRef;

    private Action<object?> CloseAction => Close;

    protected override void OnInitialized()
    {
        _inputValue = Model.DefaultValue;
        Model.RequestClose = Close;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var modulePath = await assetManifestService.GetAssetPathAsync(
            "src/interop/webawesome-dialog.ts"
        );
        _dialogModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        _dotNetRef = DotNetObjectReference.Create(this);
        await _dialogModule.InvokeVoidAsync("bindDialog", _dialogRef, _dotNetRef);
        await _dialogModule.InvokeVoidAsync("notifyContentReady");

        if (Model.Type == AppDialogType.Prompt)
        {
            await _inputRef.FocusAsync();
        }
    }

    /// <summary>
    /// 由 JS 端在对话框动画隐藏完成后回调，触发内部关闭流程
    /// </summary>
    [JSInvokable]
    public async Task HandleAfterHideFromDialog()
    {
        if (_isClosing)
        {
            return;
        }

        await CompleteCloseAsync(null);
    }

    private void HandleInput(ChangeEventArgs args)
    {
        _inputValue = args.Value?.ToString() ?? "";
    }

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && Model.Type != AppDialogType.Component)
        {
            Close(true);
        }
    }

    private void Close(object? result)
    {
        _ = CompleteCloseAsync(result);
    }

    private async Task CompleteCloseAsync(object? result)
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;

        var finalResult = result;
        if (Model.Type == AppDialogType.Prompt && result is true)
        {
            finalResult = _inputValue;
        }
        else if (Model.Type == AppDialogType.Confirm)
        {
            finalResult = result is true;
        }

        Model.TaskSource.TrySetResult(finalResult);

        if (_dialogModule != null)
        {
            await _dialogModule.InvokeVoidAsync("hideDialog", _dialogRef);
        }

        await OnClose.InvokeAsync(Model);
    }

    private string GetDialogStyle()
    {
        if (string.IsNullOrWhiteSpace(Model.Width))
        {
            return "";
        }

        return $"--width: {Model.Width};";
    }

    public async ValueTask DisposeAsync()
    {
        Model.RequestClose = null;

        if (_dialogModule != null)
        {
            try
            {
                await _dialogModule.InvokeVoidAsync("unbindDialog", _dialogRef);
                await _dialogModule.DisposeAsync();
            }
            catch
            {
                Console.WriteLine("Failed to dispose WebAwesome dialog module.");
            }
        }

        _dotNetRef?.Dispose();
    }
}
