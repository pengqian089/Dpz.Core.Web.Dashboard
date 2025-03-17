using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Picture;

public partial class Edit
{
    [Parameter]
    public string Id { get; set; }

    [Inject]
    private IPictureService PictureService { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    private bool _isPosting;

    private readonly PostPicture _picture = new();

    private List<string> _tags = [];

    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        _tags = await PictureService.GetTagsAsync();
        await base.OnInitializedAsync();
    }

    private async Task PostPictureAsync()
    {
        Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        Snackbar.Configuration.MaxDisplayedSnackbars = 10;
        if (string.IsNullOrEmpty(Id))
        {
            Snackbar.Add("参数错误", Severity.Warning);
            return;
        }

        if (_picture.Image != null && !_picture.Image.ContentType.StartsWith("image/"))
        {
            Snackbar.Add("请选择图片", Severity.Warning);
            return;
        }

        //StateHasChanged();
        _isPosting = true;
        using var content = new MultipartFormDataContent();

        if (_picture.Image != null)
        {
            var fileContent = new StreamContent(
                _picture.Image.OpenReadStream(AppTools.MaxFileSize)
            );
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(_picture.Image.ContentType);
            content.Add(content: fileContent, name: "\"image\"", fileName: _picture.Image.Name);
        }

        AppTools.DebugOutPut(_picture.AdditionsTags);
        AppTools.DebugOutPut(_picture.Tags);
        // 处理标签
        var tags =
            _picture
                .AdditionsTags?.Split(",")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList() ?? [];
        if (_picture.Tags?.Count > 0)
            tags.AddRange(_picture.Tags);
        tags = tags.Distinct().ToList();
        await PictureService.EditAsync(
            new
            {
                Id,
                Tags = tags,
                _picture.Description,
            }
        );
        Navigation.NavigateTo("/picture");
    }

    private Dictionary<string, long> _selectedFiles = new();

    private async Task OnInputFileChanged(InputFileChangeEventArgs e)
    {
        var imageFile = e.File;
        if (!imageFile.ContentType.StartsWith("image/"))
        {
            _picture.Image = null;
            Snackbar.Add("请选择图片", Severity.Warning);
            return;
        }

        _selectedFiles = new() { { imageFile.Name, imageFile.Size } };
        _picture.Image = imageFile;

        var resizedImage = await imageFile.RequestImageFileAsync(imageFile.ContentType, 1000, 1000);
        var jsImageStream = resizedImage.OpenReadStream();
        var dotnetImageStream = new DotNetStreamReference(jsImageStream);
        await JsRuntime.InvokeVoidAsync(
            "setImageUsingStreaming",
            "imagePreview",
            dotnetImageStream
        );
    }

    private string _imageSrc = "";

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        var picture = await PictureService.GetPictureAsync(Id);
        if (picture != null)
        {
            _picture.Description = picture.Description;
            _picture.Tags = picture.Tags.ToList();
            _selectedFiles = new Dictionary<string, long>
            {
                { picture.AccessUrl ?? "暂无图片", picture.Length },
            };
            _imageSrc = picture.AccessUrl;
        }

        _isLoading = false;
        await base.OnParametersSetAsync();
    }

    private class PostPicture
    {
        public string Description { get; set; }

        public List<string> Tags { get; set; }

        public string AdditionsTags { get; set; }

        public IBrowserFile Image { get; set; }
    }

    private void OnTagsSelected(IEnumerable<string> tags)
    {
        _picture.Tags = tags.ToList();
    }
}
