using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Upload;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class MarkdownEditor(
    IHttpService httpService,
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService,
    ILocalStorageService localStorageService
) : ComponentBase, IAsyncDisposable
{
    [Parameter]
    [EditorRequired]
    public required string Markdown { get; set; }

    [Parameter]
    public required string UploadAction { get; set; }

    [Parameter]
    public int? Height { get; set; }

    [Parameter]
    public string HeightUnit { get; set; } = "px";

    [Parameter]
    public EventCallback<string>? OnImageUploading { get; set; }

    [Parameter]
    public EventCallback<string>? OnImageUploaded { get; set; }

    private string HeightStyle => Height == null ? "" : $"height:{Height}{HeightUnit}";

    private readonly string _editorId = Guid.NewGuid().ToString("N");
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MarkdownEditor>? _objRef;
    private bool _editOnly;
    private bool _isUploading;
    private int _uploadProgress;
    private bool _editorInitialized;

    protected override async Task OnInitializedAsync()
    {
        _editOnly = await localStorageService.GetItemAsync<bool>("markdown-edit-only");

        try
        {
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/editors/markdown-editor.ts"
            );
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load MarkdownEditor module: {ex.Message}");
        }

        _objRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_editorInitialized || _jsModule == null || _objRef == null)
        {
            return;
        }

        try
        {
            await _jsModule.InvokeVoidAsync(
                "createEditor",
                _editorId,
                Markdown,
                _editOnly,
                _objRef
            );
            _editorInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create MarkdownEditor: {ex.Message}");
        }
    }

    public async Task<string> GetValueAsync()
    {
        if (_jsModule == null || !_editorInitialized)
        {
            return Markdown;
        }

        return await _jsModule.InvokeAsync<string>("getMarkdown", _editorId);
    }

    public async Task ToggleEditModeAsync()
    {
        _editOnly = !_editOnly;
        await localStorageService.SetItemAsync("markdown-edit-only", _editOnly);

        if (_jsModule != null && _editorInitialized)
        {
            await _jsModule.InvokeVoidAsync("setReadonly", _editorId, _editOnly);
        }
    }

    [JSInvokable]
    public async Task<string> UploadImage(
        IJSStreamReference streamRef,
        string fileName,
        string contentType
    )
    {
        _isUploading = true;
        _uploadProgress = 0;
        StateHasChanged();
        try
        {
            if (OnImageUploading != null)
            {
                await OnImageUploading.Value.InvokeAsync("开始上传图片...");
            }

            using var stream = await streamRef.OpenReadStreamAsync(AppTools.MaxFileSize);
            var files = new List<UploadFilePart> { new("image", fileName, contentType, stream) };
            var progress = new Progress<int>(value =>
            {
                _uploadProgress = value;
                StateHasChanged();
            });

            var result = await httpService.PostFileWithProgressAsync<UploadImageResult>(
                UploadAction,
                files,
                null,
                progress
            );

            if (result != null && !string.IsNullOrWhiteSpace(result.Url))
            {
                if (OnImageUploaded != null)
                {
                    await OnImageUploaded.Value.InvokeAsync(result.Url);
                }
                return result.Url;
            }
        }
        catch (Exception)
        {
            if (OnImageUploaded != null)
            {
                await OnImageUploaded.Value.InvokeAsync(string.Empty);
            }
        }
        finally
        {
            _isUploading = false;
            StateHasChanged();
        }

        return string.Empty;
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            if (_editorInitialized)
            {
                await _jsModule.InvokeVoidAsync("destroy", _editorId);
            }

            await _jsModule.DisposeAsync();
        }

        _objRef?.Dispose();
    }

    private record UploadImageResult(string? Url);
}
