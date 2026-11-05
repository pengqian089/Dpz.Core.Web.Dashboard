using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Upload;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class MarkdownEditor(
    IHttpService httpService,
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService,
    ILocalStorageService localStorageService,
    IAppDialogService dialogService,
    ILogger<MarkdownEditor> logger
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

    [Parameter]
    public MarkdownImageMode ImageMode { get; set; } = MarkdownImageMode.Inline;

    private const int DefaultHeight = 800;

    /// <summary>
    /// 单次最多允许选择的图片数量，避免一次请求过大。
    /// </summary>
    private const int MaxGalleryUploadCount = 30;

    private string HeightStyle => $"height:{Height ?? DefaultHeight}{HeightUnit}";
    private string GalleryId => $"markdown-gallery-{_editorId}";
    private string GalleryInputId => $"markdown-gallery-input-{_editorId}";

    private readonly string _editorId = Guid.NewGuid().ToString("N");
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _photoSwipeModule;
    private DotNetObjectReference<MarkdownEditor>? _objRef;
    private bool _editOnly;
    private bool _isUploading;
    private int _uploadProgress;
    private bool _editorInitialized;
    private bool _photoSwipeInitialized;
    private bool _shouldRefreshPhotoSwipe;
    private string? _uploadError;
    private string _editorInitialMarkdown = "";
    private int _inputFileKey;
    private readonly List<ImageMetadata> _uploadedImages = [];
    private readonly List<GalleryImage> _galleryImages = [];

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected override async Task OnInitializedAsync()
    {
        if (ImageMode == MarkdownImageMode.Gallery)
        {
            InitializeGalleryImages();
        }
        else
        {
            _uploadedImages.AddRange(Images ?? []);
        }

        // 画廊模式下图片语法仍保留在 Markdown 中（源码模式可见），
        // 只是可视化模式通过 CSS 隐藏图片节点，图片统一由底部画廊管理。
        _editorInitialMarkdown = Markdown;

        _editOnly = await localStorageService.GetItemAsync<bool>("markdown-edit-only");

        try
        {
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/editors/markdown-editor.ts"
            );
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);

            if (ImageMode == MarkdownImageMode.Gallery)
            {
                var photoSwipeModulePath = await assetManifestService.GetAssetPathAsync(
                    "src/photoswipe-gallery.ts"
                );
                _photoSwipeModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    photoSwipeModulePath
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load MarkdownEditor module: {ex.Message}");
        }

        _objRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_editorInitialized && _jsModule != null && _objRef != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync(
                    "createEditor",
                    _editorId,
                    _editorInitialMarkdown,
                    _editOnly,
                    _objRef,
                    ImageMode.ToString().ToLowerInvariant(),
                    ImageMode == MarkdownImageMode.Gallery ? GalleryInputId : null
                );
                _editorInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create MarkdownEditor: {ex.Message}");
            }
        }

        if (ImageMode == MarkdownImageMode.Gallery && _galleryImages.Count > 0)
        {
            await EnsurePhotoSwipeAsync();
        }
    }

    public async Task<string> GetValueAsync()
    {
        var markdown = _editorInitialMarkdown;
        if (_jsModule != null && _editorInitialized)
        {
            try
            {
                markdown = await _jsModule.InvokeAsync<string>("getMarkdown", _editorId);
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
                markdown = _editorInitialMarkdown;
            }
        }

        if (ImageMode != MarkdownImageMode.Gallery)
        {
            return markdown;
        }

        // 源码模式下用户可能手动补了图片语法，这里把它们并入画廊，避免落库时被丢弃。
        MergeMarkdownImages(markdown);
        return NormalizeGalleryMarkdown(markdown, _galleryImages);
    }

    public List<ImageMetadata> GetUploadedImages()
    {
        if (ImageMode == MarkdownImageMode.Gallery)
        {
            return _galleryImages.Select(image => image.Metadata).ToList();
        }

        return [.. _uploadedImages];
    }

    public async Task ToggleEditModeAsync()
    {
        _editOnly = !_editOnly;
        await localStorageService.SetItemAsync("markdown-edit-only", _editOnly);

        if (_jsModule != null && _editorInitialized)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("setReadonly", _editorId, _editOnly);
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions
                    {
                        Title = "错误",
                        Message = $"切换编辑模式失败：{ex.Message}",
                    }
                );
            }
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

    /// <summary>
    /// 供可视化编辑器（inline 模式）的拖拽/粘贴/图片块上传使用。
    /// </summary>
    [JSInvokable]
    public async Task<string[]> UploadImages(
        IJSStreamReference[] streamRefs,
        string[] fileNames,
        string[] contentTypes
    )
    {
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

            var urls = await PostImagesAsync(files, fileNames);
            await NotifyUploadedAsync(urls);
            return urls;
        }
        catch (Exception ex)
        {
            _uploadError = $"图片上传失败：{ex.Message}";
            logger.LogError(ex, "Failed to read image streams for upload.");
            return [];
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// 画廊模式的多图上传入口。
    ///
    /// 直接读取 Blazor 的 <see cref="IBrowserFile"/>，不再经过 JS 侧的
    /// <c>JSStreamReference</c> 数组，避免浏览器一次性选择多张图片时上传链路失效；
    /// 选中的图片会统一追加到底部画廊，并在保存时写回 Markdown 末尾。
    /// </summary>
    private async Task UploadGalleryImagesAsync(InputFileChangeEventArgs e)
    {
        if (ImageMode != MarkdownImageMode.Gallery || _isUploading)
        {
            return;
        }

        var streams = new List<Stream>();
        try
        {
            var files = new List<UploadFilePart>();
            var fileNames = new List<string>();
            foreach (var file in e.GetMultipleFiles(MaxGalleryUploadCount))
            {
                if (!IsImageFile(file))
                {
                    continue;
                }

                var stream = file.OpenReadStream(AppTools.MaxFileSize);
                streams.Add(stream);
                fileNames.Add(file.Name);
                files.Add(new UploadFilePart("image", file.Name, file.ContentType, stream));
            }

            if (files.Count == 0)
            {
                _uploadError = "请选择图片文件。";
                return;
            }

            if (OnImageUploading != null)
            {
                await OnImageUploading.Value.InvokeAsync("开始上传图片...");
            }

            var urls = await PostImagesAsync(files, [.. fileNames]);
            if (urls.Length > 0)
            {
                await SyncEditorMarkdownAsync();
                await NotifyUploadedAsync(urls);
            }
        }
        catch (Exception ex)
        {
            _uploadError = $"图片上传失败：{ex.Message}";
            logger.LogError(ex, "Failed to upload gallery images.");
        }
        finally
        {
            // 重建 InputFile，保证再次选择同一批文件时仍能触发 change。
            _inputFileKey++;
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// 把画廊的最新状态同步回编辑器。
    ///
    /// 画廊是图片的唯一来源：这里不能反过来从编辑器内容收集图片，否则刚被删除的图片
    /// 会因为编辑器里还留着旧语法而被重新收集回来（表现为“删不掉、被排到最后”）。
    /// 源码模式手动补的图片只在保存时由 <see cref="GetValueAsync"/> 合并。
    /// </summary>
    private async Task SyncEditorMarkdownAsync()
    {
        if (ImageMode != MarkdownImageMode.Gallery || _jsModule == null || !_editorInitialized)
        {
            return;
        }

        string markdown;
        try
        {
            markdown = await _jsModule.InvokeAsync<string>("getMarkdown", _editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read markdown before syncing gallery images.");
            return;
        }

        var normalizedMarkdown = NormalizeGalleryMarkdown(markdown, _galleryImages);
        try
        {
            await _jsModule.InvokeVoidAsync("setMarkdown", _editorId, normalizedMarkdown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync gallery images into the editor.");
        }
    }

    private async Task NotifyUploadedAsync(IReadOnlyCollection<string> urls)
    {
        if (OnImageUploaded == null)
        {
            return;
        }

        foreach (var url in urls)
        {
            await OnImageUploaded.Value.InvokeAsync(url);
        }
    }

    /// <summary>
    /// 统一的上传实现：展示进度、解析服务端响应并把图片登记到画廊。
    /// </summary>
    private async Task<string[]> PostImagesAsync(
        IReadOnlyList<UploadFilePart> files,
        IReadOnlyList<string> fileNames
    )
    {
        _isUploading = true;
        _uploadProgress = 0;
        _uploadError = null;
        StateHasChanged();
        try
        {
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
            if (images.Length == 0)
            {
                _uploadError = "上传失败：服务器没有返回图片地址。";
                return [];
            }

            _uploadedImages.AddRange(images);
            AddUploadedGalleryImages(images, [.. fileNames]);
            return images.Select(image => image.Url).ToArray();
        }
        catch (Exception ex)
        {
            _uploadError = $"图片上传失败：{ex.Message}";
            logger.LogError(ex, "Failed to upload images to {UploadAction}.", UploadAction);
            return [];
        }
        finally
        {
            _isUploading = false;
            StateHasChanged();
        }
    }

    private async Task RemoveGalleryImageAsync(string url)
    {
        if (ImageMode != MarkdownImageMode.Gallery)
        {
            return;
        }

        _galleryImages.RemoveAll(image =>
            string.Equals(image.Metadata.Url, url, StringComparison.Ordinal)
        );
        _uploadedImages.RemoveAll(image => string.Equals(image.Url, url, StringComparison.Ordinal));
        _shouldRefreshPhotoSwipe = true;
        await SyncEditorMarkdownAsync();
    }

    private static bool IsImageFile(IBrowserFile file)
    {
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var extension = Path.GetExtension(file.Name).TrimStart('.');
        return AppTools.ImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private void MergeMarkdownImages(string markdown)
    {
        foreach (var image in ExtractMarkdownImages(markdown))
        {
            AddGalleryImage(CreateFallbackImageMetadata(image.Url), image.Alt);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_photoSwipeModule != null)
        {
            try
            {
                await _photoSwipeModule.InvokeVoidAsync("destroyPhotoViewer");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to destroy photo swipe module.");
            }
            await _photoSwipeModule.DisposeAsync();
        }

        if (_jsModule != null)
        {
            if (_editorInitialized)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("destroy", _editorId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to destroy markdown editor.");
                }
            }

            await _jsModule.DisposeAsync();
        }

        _objRef?.Dispose();
    }

    private async Task EnsurePhotoSwipeAsync()
    {
        if (_photoSwipeModule == null)
        {
            return;
        }

        if (_photoSwipeInitialized && !_shouldRefreshPhotoSwipe)
        {
            return;
        }

        try
        {
            await _photoSwipeModule.InvokeVoidAsync("initPhotoSwipe", $"#{GalleryId}");
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync(
                new AppDialogOptions
                {
                    Title = "错误",
                    Message = $"初始化图片查看器失败：{ex.Message}",
                }
            );
        }
        _photoSwipeInitialized = true;
        _shouldRefreshPhotoSwipe = false;
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

    private void InitializeGalleryImages()
    {
        var imageLookup = (Images ?? [])
            .GroupBy(image => image.Url)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var markdownImages = ExtractMarkdownImages(Markdown);

        foreach (var markdownImage in markdownImages)
        {
            var metadata = imageLookup.TryGetValue(markdownImage.Url, out var image)
                ? image
                : CreateFallbackImageMetadata(markdownImage.Url);
            AddGalleryImage(metadata, markdownImage.Alt);
        }

        foreach (var image in Images ?? [])
        {
            AddGalleryImage(image, CreateImageAlt(image.Url));
        }

        _uploadedImages.AddRange(_galleryImages.Select(image => image.Metadata));
    }

    private void AddUploadedGalleryImages(ImageMetadata[] images, string[] fileNames)
    {
        if (ImageMode != MarkdownImageMode.Gallery)
        {
            return;
        }

        for (var i = 0; i < images.Length; i++)
        {
            AddGalleryImage(images[i], GetUploadImageAlt(fileNames, i));
        }

        _shouldRefreshPhotoSwipe = true;
    }

    private void AddGalleryImage(ImageMetadata image, string alt)
    {
        if (string.IsNullOrWhiteSpace(image.Url))
        {
            return;
        }

        if (
            _galleryImages.Any(item =>
                string.Equals(item.Metadata.Url, image.Url, StringComparison.Ordinal)
            )
        )
        {
            return;
        }

        _galleryImages.Add(new GalleryImage(image, string.IsNullOrWhiteSpace(alt) ? "image" : alt));
    }

    private static string NormalizeGalleryMarkdown(
        string markdown,
        IReadOnlyCollection<GalleryImage> galleryImages
    )
    {
        var body = RemoveMarkdownImages(markdown).Trim();
        if (galleryImages.Count == 0)
        {
            return body;
        }

        var imagesMarkdown = string.Join(
            "\n\n",
            galleryImages.Select(image => CreateImageMarkdown(image.Alt, image.Metadata.Url))
        );

        return string.IsNullOrWhiteSpace(body) ? imagesMarkdown : $"{body}\n\n{imagesMarkdown}";
    }

    private static string RemoveMarkdownImages(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "";
        }

        var images = ExtractMarkdownImages(markdown);
        if (images.Count == 0)
        {
            return markdown;
        }

        var builder = new StringBuilder(markdown);
        foreach (var image in images.OrderByDescending(image => image.Start))
        {
            builder.Remove(image.Start, image.End - image.Start);
        }

        return builder
            .ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Aggregate(new StringBuilder(), AppendNormalizedLine)
            .ToString()
            .Trim();
    }

    private static StringBuilder AppendNormalizedLine(StringBuilder builder, string line)
    {
        if (builder.Length == 0)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                builder.Append(line);
            }

            return builder;
        }

        var lastLineEmpty =
            builder.Length >= 2
            && builder[builder.Length - 1] == '\n'
            && builder[builder.Length - 2] == '\n';
        if (string.IsNullOrWhiteSpace(line) && lastLineEmpty)
        {
            return builder;
        }

        builder.Append('\n').Append(line);
        return builder;
    }

    private static List<MarkdownImage> ExtractMarkdownImages(string markdown)
    {
        var images = new List<MarkdownImage>();
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return images;
        }

        var index = 0;
        while (index < markdown.Length - 3)
        {
            if (markdown[index] != '!' || markdown[index + 1] != '[')
            {
                index++;
                continue;
            }

            if (!TryReadMarkdownImage(markdown, index, out var image))
            {
                index++;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(image.Url))
            {
                images.Add(image);
            }

            index = image.End;
        }

        return images;
    }

    private static bool TryReadMarkdownImage(string markdown, int start, out MarkdownImage image)
    {
        image = default;
        var labelEnd = FindClosingBracket(markdown, start + 1, '[', ']');
        if (labelEnd < 0 || labelEnd + 1 >= markdown.Length || markdown[labelEnd + 1] != '(')
        {
            return false;
        }

        var linkEnd = FindClosingBracket(markdown, labelEnd + 1, '(', ')');
        if (linkEnd < 0)
        {
            return false;
        }

        var alt = markdown.Substring(start + 2, labelEnd - start - 2);
        var linkContent = markdown.Substring(labelEnd + 2, linkEnd - labelEnd - 2);
        var url = ExtractMarkdownImageUrl(linkContent);
        image = new MarkdownImage(start, linkEnd + 1, alt, url);
        return true;
    }

    private static int FindClosingBracket(
        string value,
        int openIndex,
        char openChar,
        char closeChar
    )
    {
        var depth = 0;
        var escaped = false;
        for (var i = openIndex; i < value.Length; i++)
        {
            var current = value[i];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                continue;
            }

            if (current == openChar)
            {
                depth++;
                continue;
            }

            if (current != closeChar)
            {
                continue;
            }

            depth--;
            if (depth == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private static string ExtractMarkdownImageUrl(string linkContent)
    {
        var trimmed = linkContent.Trim();
        if (trimmed.Length == 0)
        {
            return "";
        }

        if (trimmed[0] == '<')
        {
            var end = trimmed.IndexOf('>');
            return end > 0 ? trimmed[1..end] : "";
        }

        var whitespaceIndex = trimmed.IndexOfAny([' ', '\t', '\r', '\n']);
        return whitespaceIndex > 0 ? trimmed[..whitespaceIndex] : trimmed;
    }

    private static string CreateImageMarkdown(string alt, string url)
    {
        return $"![{EscapeImageAlt(alt)}]({url})";
    }

    private static string EscapeImageAlt(string alt)
    {
        return (string.IsNullOrWhiteSpace(alt) ? "image" : alt)
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("[", "\\[", StringComparison.Ordinal)
            .Replace("]", "\\]", StringComparison.Ordinal);
    }

    private static string GetUploadImageAlt(string[] fileNames, int index)
    {
        var fileName = GetUploadFileName(fileNames, index);
        return Path.GetFileNameWithoutExtension(fileName).Trim();
    }

    private static string CreateImageAlt(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return "image";
        }

        var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
        return string.IsNullOrWhiteSpace(fileName) ? "image" : fileName;
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

        return [CreateFallbackImageMetadata(image.Url)];
    }

    private static ImageMetadata CreateFallbackImageMetadata(string url)
    {
        return new ImageMetadata
        {
            Url = url,
            Width = 0,
            Height = 0,
            Frames = 0,
            Size = 0,
            Format = ImageFormat.Unknown,
        };
    }

    private sealed record GalleryImage(ImageMetadata Metadata, string Alt);

    private readonly record struct MarkdownImage(int Start, int End, string Alt, string Url);

    private sealed record UploadImageResult(string? Url);
}
