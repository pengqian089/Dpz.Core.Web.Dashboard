using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class CodeEditor(IJSRuntime jsRuntime, IAssetManifestService assetManifestService)
    : ComponentBase,
        IAsyncDisposable
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
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/editors/code-editor.ts"
            );
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        }

        if (!_initialized)
        {
            await _module.InvokeVoidAsync(
                "createEditor",
                _elementId,
                new CodeEditorOptions(Value, Language, ReadOnly)
            );
            RememberParameters();
            _initialized = true;
            return;
        }

        if (HasParameterChanges())
        {
            await _module.InvokeVoidAsync(
                "updateEditor",
                _elementId,
                new CodeEditorOptions(Value, Language, ReadOnly)
            );
            RememberParameters();
        }
    }

    public async Task<string> GetValueAsync()
    {
        if (_module == null || !_initialized)
        {
            return Value;
        }

        return await _module.InvokeAsync<string>("getValue", _elementId);
    }

    public async Task InsertValueAsync(string value)
    {
        if (_module == null || !_initialized)
        {
            return;
        }

        await _module.InvokeVoidAsync("insertValue", _elementId, value);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            if (_initialized)
            {
                await _module.InvokeVoidAsync("destroy", _elementId);
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
