using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Upload;
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
    private List<string> _selectedTags = [];
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
        _uploadProgress = 0;
        StateHasChanged();

        try
        {
            var fields = new List<UploadFormField>();
            foreach (var tag in _selectedTags.Distinct())
            {
                fields.Add(new UploadFormField("tags", tag));
            }
            fields.Add(new UploadFormField("description", _picture.Description ?? ""));

            await using var stream = _picture.Image.OpenReadStream(AppTools.MaxFileSize);
            var files = new List<UploadFilePart>
            {
                new("image", _picture.Image.Name, _picture.Image.ContentType, stream),
            };
            var progress = new Progress<int>(value =>
            {
                _uploadProgress = value;
                StateHasChanged();
            });

            await pictureService.UploadWithProgressAsync(files, fields, progress);

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

    private void HandleNewTagAdded(string tag)
    {
        dialogService.Toast($"标签 '{tag}' 已添加", ToastType.Success);
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
        public IBrowserFile? Image { get; set; }
    }
}
