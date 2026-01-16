using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class MarkdownEditor(
    IHttpService httpService,
    IJSRuntime jsRuntime,
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

    private bool _editOnly;

    private DotNetObjectReference<MarkdownEditor>? _objRef;

    private bool _isUploading;

    private bool _editorInitialized;

    protected override async Task OnInitializedAsync()
    {
        _editOnly = await localStorageService.GetItemAsync<bool>("markdown-edit-only");

        try
        {
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Shared/Components/MarkdownEditor.razor.js"
            );
            Console.WriteLine($"MarkdownEditor JS module loaded for {_editorId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load JS module: {ex.Message}");
        }

        _objRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_editorInitialized && _jsModule != null)
        {
            try
            {
                Console.WriteLine($"Calling createEditor for {_editorId}");
                await _jsModule.InvokeVoidAsync(
                    "createEditor",
                    _editorId,
                    Markdown,
                    _editOnly,
                    _objRef
                );
                _editorInitialized = true;
                Console.WriteLine($"createEditor called successfully for {_editorId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create editor: {ex.Message}");
            }
        }
    }

    public async Task<string> GetValueAsync()
    {
        if (_jsModule == null)
        {
            return string.Empty;
        }
        return await _jsModule.InvokeAsync<string>("getMarkdown");
    }

    public async Task ToggleEditModeAsync()
    {
        _editOnly = !_editOnly;
        await localStorageService.SetItemAsync("markdown-edit-only", _editOnly);

        if (_jsModule != null)
        {
            var currentMarkdown = await _jsModule.InvokeAsync<string>("getMarkdown");
            await _jsModule.InvokeVoidAsync("destroy");
            await _jsModule.InvokeVoidAsync(
                "createEditor",
                _editorId,
                currentMarkdown,
                _editOnly,
                _objRef
            );
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
        StateHasChanged();
        try
        {
            if (OnImageUploading != null)
            {
                await OnImageUploading.Value.InvokeAsync("开始上传图片...");
            }

            using var stream = await streamRef.OpenReadStreamAsync(AppTools.MaxFileSize);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "\"image\"", fileName);

            var result = await httpService.PostFileAsync<UploadImageResult>(UploadAction, content);

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
            await _jsModule.InvokeVoidAsync("destroy");
            await _jsModule.DisposeAsync();
        }

        _objRef?.Dispose();
    }

    private record UploadImageResult(string? Url);
}
