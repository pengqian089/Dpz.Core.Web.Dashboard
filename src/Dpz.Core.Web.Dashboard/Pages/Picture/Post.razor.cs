using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages.Picture;

public partial class Post(
    IPictureService pictureService,
    IAppDialogService dialogService,
    NavigationManager navigation,
    IJSRuntime jsRuntime
) : IAsyncDisposable
{
    private bool _isPosting;
    private readonly PostPictureModel _picture = new();
    private List<string> _tags = [];
    private Dictionary<string, long> _selectedFiles = [];
    private int _uploadProgress;
    private IJSObjectReference? _jsModule;

    protected override async Task OnInitializedAsync()
    {
        _tags = await pictureService.GetTagsAsync();
        _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./Pages/Picture/Post.razor.js"
        );
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("initPhotoSwipe", ".pswp-gallery");
        }
    }

    private async Task PostPictureAsync()
    {
        if (_picture.Image == null || !_picture.Image.ContentType.StartsWith("image/"))
        {
            dialogService.Toast("请选择图片", ToastType.Warning);
            return;
        }

        _isPosting = true;
        StateHasChanged();

        try
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(
                _picture.Image.OpenReadStream(AppTools.MaxFileSize)
            );
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(_picture.Image.ContentType);
            content.Add(fileContent, "\"image\"", _picture.Image.Name);

            var tags =
                _picture
                    .AdditionsTags?.Split(",")
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList() ?? [];

            if (_picture.Tags?.Count > 0)
            {
                tags.AddRange(_picture.Tags);
            }

            foreach (var tag in tags.Distinct())
            {
                var tagContent = new StringContent(tag);
                tagContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(
                    "form-data"
                )
                {
                    Name = "\"tags\"",
                };
                content.Add(tagContent);
            }

            var descContent = new StringContent(_picture.Description ?? "");
            content.Add(descContent, "\"description\"");

            await pictureService.UploadAsync(content);
            dialogService.Toast("上传成功", ToastType.Success);
            navigation.NavigateTo("/picture");
        }
        catch (Exception ex)
        {
            dialogService.Toast($"上传失败: {ex.Message}", ToastType.Error);
            _isPosting = false;
            StateHasChanged();
        }
    }

    private async Task OnInputFileChanged(InputFileChangeEventArgs e)
    {
        var imageFile = e.File;
        if (!imageFile.ContentType.StartsWith("image/"))
        {
            _picture.Image = null;
            dialogService.Toast("请选择图片", ToastType.Warning);
            return;
        }

        _selectedFiles = new() { { imageFile.Name, imageFile.Size } };
        _picture.Image = imageFile;

        if (_jsModule != null)
        {
            var resizedImage = await imageFile.RequestImageFileAsync(
                imageFile.ContentType,
                1000,
                1000
            );
            var jsImageStream = resizedImage.OpenReadStream(AppTools.MaxFileSize);
            var dotnetImageStream = new DotNetStreamReference(jsImageStream);
            await _jsModule.InvokeVoidAsync("setImagePreview", "imagePreview", dotnetImageStream);
        }
    }

    private void ToggleTag(string tag)
    {
        _picture.Tags ??= [];

        if (_picture.Tags.Contains(tag))
        {
            _picture.Tags.Remove(tag);
        }
        else
        {
            _picture.Tags.Add(tag);
        }

        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            await _jsModule.DisposeAsync();
        }
    }

    private class PostPictureModel
    {
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public string? AdditionsTags { get; set; }
        public IBrowserFile? Image { get; set; }
    }
}
