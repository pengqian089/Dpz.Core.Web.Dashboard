using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class CodeEditor(
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService,
    IAppDialogService dialogService,
    ILogger<CodeEditor> logger
) : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public string Value { get; set; } = "";

    [Parameter]
    public string Language { get; set; } = "plaintext";

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public string CssClass { get; set; } = "code-editor";

    [Parameter]
    public string? Style { get; set; }

    private readonly string _elementId = $"code-editor-{Guid.NewGuid():N}";
    private IJSObjectReference? _module;
    private bool _initialized;
    private string _lastValue = "";
    private string _lastLanguage = "";
    private bool _lastReadOnly;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_module == null)
        {
            try
            {
                var modulePath = await assetManifestService.GetAssetPathAsync(
                    "src/editors/code-editor.ts"
                );
                _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions
                    {
                        Title = "错误",
                        Message = $"代码编辑器脚本加载失败：{ex.Message}",
                    }
                );
            }
        }

        if (!_initialized)
        {
            if (_module != null)
            {
                try
                {
                    await _module.InvokeVoidAsync(
                        "createEditor",
                        _elementId,
                        new CodeEditorOptions(Value, Language, ReadOnly)
                    );
                }
                catch (Exception ex)
                {
                    await dialogService.ShowAlertAsync(
                        new AppDialogOptions
                        {
                            Title = "错误",
                            Message = $"代码编辑器初始化失败：{ex.Message}",
                        }
                    );
                }
            }
            RememberParameters();
            _initialized = true;
            return;
        }

        if (HasParameterChanges() && _module != null)
        {
            try
            {
                await _module.InvokeVoidAsync(
                    "updateEditor",
                    _elementId,
                    new CodeEditorOptions(Value, Language, ReadOnly)
                );
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions
                    {
                        Title = "错误",
                        Message = $"代码编辑器更新失败：{ex.Message}",
                    }
                );
            }
            RememberParameters();
        }
    }

    public async Task<string> GetValueAsync()
    {
        if (_module == null || !_initialized)
        {
            return Value;
        }

        try
        {
            return await _module.InvokeAsync<string>("getValue", _elementId);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync(
                new AppDialogOptions
                {
                    Title = "错误",
                    Message = $"获取编辑器内容失败：{ex.Message}",
                }
            );
            return Value;
        }
    }

    public async Task InsertValueAsync(string value)
    {
        if (_module == null || !_initialized)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("insertValue", _elementId, value);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync(
                new AppDialogOptions
                {
                    Title = "错误",
                    Message = $"插入编辑器内容失败：{ex.Message}",
                }
            );
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            if (_initialized)
            {
                try
                {
                    await _module.InvokeVoidAsync("destroy", _elementId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to destroy code editor.");
                }
            }

            await _module.DisposeAsync();
        }
    }

    private bool HasParameterChanges()
    {
        return _lastValue != Value || _lastLanguage != Language || _lastReadOnly != ReadOnly;
    }

    private void RememberParameters()
    {
        _lastValue = Value;
        _lastLanguage = Language;
        _lastReadOnly = ReadOnly;
    }

    private record CodeEditorOptions(string Value, string Language, bool ReadOnly);
}
