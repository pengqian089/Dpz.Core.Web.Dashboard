using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components.Dialog;

public partial class DialogBox(IJSRuntime jsRuntime) : IAsyncDisposable
{
    [Parameter]
    public DialogModel Model { get; set; } = new();

    [Parameter]
    public EventCallback<DialogModel> OnClose { get; set; }

    private bool _isVisible;
    private string _inputValue = "";
    private ElementReference _inputRef;
    private ElementReference _confirmBtnRef;
    private ElementReference _dialogRef;
    private IJSObjectReference? _dialogModule;

    protected override async Task OnInitializedAsync()
    {
        _inputValue = Model.DefaultValue;

        // 注册关闭操作
        Model.RequestCloseAction = () => Close(null);

        // Trigger animation
        await Task.Delay(10);
        _isVisible = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dialogModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./js/modules/dialog-interop.js"
            );

            if (Model.Type == DialogType.Prompt)
            {
                await _inputRef.FocusAsync();
            }
            else if (Model.Type != DialogType.Component)
            {
                await _confirmBtnRef.FocusAsync();
            }

            // 对话框打开时 DOM 管理器已暂停，手动触发 LazyLoad 更新以加载图片
            if (_dialogModule != null)
            {
                await _dialogModule.InvokeVoidAsync("updateLazyLoad");
            }
        }
    }

    private void HandleOverlayClick()
    {
        if (Model.MaskToClose)
        {
            Close(null);
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (Model.Type != DialogType.Component)
            {
                Close(true);
            }
        }
    }

    private async void Close(object? result)
    {
        _isVisible = false;
        StateHasChanged();

        // Wait for animation
        await Task.Delay(300);

        if (Model.Type == DialogType.Prompt && result is true)
        {
            Model.TaskSource.TrySetResult(_inputValue);
        }
        else if (Model.Type == DialogType.Confirm)
        {
            Model.TaskSource.TrySetResult(result is true);
        }
        else
        {
            Model.TaskSource.TrySetResult(result);
        }

        await OnClose.InvokeAsync(Model);
    }

    public async ValueTask DisposeAsync()
    {
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
