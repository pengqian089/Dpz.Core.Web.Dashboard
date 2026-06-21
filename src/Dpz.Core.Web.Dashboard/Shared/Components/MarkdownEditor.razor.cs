using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
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

    [Parameter]
    public IReadOnlyCollection<ImageMetadata>? Images { get; set; }

    private const int DefaultHeight = 600;

    private string HeightStyle => $"height:{Height ?? DefaultHeight}{HeightUnit}";

    private readonly string _editorId = Guid.NewGuid().ToString("N");
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MarkdownEditor>? _objRef;
    private bool _editOnly;
    private bool _isUploading;
    private int _uploadProgress;
    private bool _editorInitialized;
    private string? _uploadError;
    private readonly List<ImageMetadata> _uploadedImages = [];

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected override async Task OnInitializedAsync()
    {
        _uploadedImages.AddRange(Images ?? []);
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

    public List<ImageMetadata> GetUploadedImages()
    {
        return [.. _uploadedImages];
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
        var urls = await UploadImages([streamRef], [fileName], [contentType]);
        return urls.FirstOrDefault() ?? string.Empty;
    }

    [JSInvokable]
    public async Task<string[]> UploadImages(
        IJSStreamReference[] streamRefs,
        string[] fileNames,
        string[] contentTypes
    )
    {
        _isUploading = true;
        _uploadProgress = 0;
        _uploadError = null;
        StateHasChanged();
        var streams = new List<Stream>();
        try
        {
            if (OnImageUploading != null)
            {
                await OnImageUploading.Value.InvokeAsync("开始上传图片...");
            }

            var files = new List<UploadFilePart>(streamRefs.Length);
            for (var i = 0; i < streamRefs.Length; i++)
            {
                var stream = await streamRefs[i].OpenReadStreamAsync(AppTools.MaxFileSize);
                streams.Add(stream);
                files.Add(
                    new UploadFilePart(
                        "image",
                        GetUploadFileName(fileNames, i),
                        GetUploadContentType(contentTypes, i),
                        stream
                    )
                );
            }

            var progress = new Progress<int>(value =>
            {
                _uploadProgress = value;
                StateHasChanged();
            });

            var response = await httpService.PostFileWithProgressAsync<string>(
                UploadAction,
                files,
                null,
                progress
            );
            var images = ParseUploadImages(response);
            _uploadedImages.AddRange(images);
            var urls = images.Select(image => image.Url).ToArray();

            foreach (var url in urls)
            {
                if (OnImageUploaded != null)
                {
                    await OnImageUploaded.Value.InvokeAsync(url);
                }
            }

            return urls;
        }
        catch (Exception ex)
        {
            _uploadError = ex.Message;
            if (OnImageUploaded != null)
            {
                await OnImageUploaded.Value.InvokeAsync(string.Empty);
            }
        }
        finally
        {
            _isUploading = false;
            StateHasChanged();
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }

        return [];
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

    private static string GetUploadFileName(string[] fileNames, int index)
    {
        return index < fileNames.Length && !string.IsNullOrWhiteSpace(fileNames[index])
            ? fileNames[index]
            : $"image-{index + 1}";
    }

    private static string GetUploadContentType(string[] contentTypes, int index)
    {
        return index < contentTypes.Length && !string.IsNullOrWhiteSpace(contentTypes[index])
            ? contentTypes[index]
            : "application/octet-stream";
    }

    private static ImageMetadata[] ParseUploadImages(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return [];
        }

        try
        {
            var images = JsonSerializer.Deserialize<List<ImageMetadata>>(
                response,
                JsonSerializerOptions
            );
            return images?.Where(image => !string.IsNullOrWhiteSpace(image.Url)).ToArray() ?? [];
        }
        catch (JsonException)
        {
            var image = JsonSerializer.Deserialize<UploadImageResult>(
                response,
                JsonSerializerOptions
            );
            return CreateFallbackImageMetadata(image);
        }
    }

    private static ImageMetadata[] CreateFallbackImageMetadata(UploadImageResult? image)
    {
        if (image == null || string.IsNullOrWhiteSpace(image.Url))
        {
            return [];
        }

        return
        [
            new ImageMetadata
            {
                Url = image.Url,
                Width = 0,
                Height = 0,
                Frames = 0,
                Size = 0,
                Format = ImageFormat.Unknown,
            },
        ];
    }

    private sealed record UploadImageResult(string? Url);
}
