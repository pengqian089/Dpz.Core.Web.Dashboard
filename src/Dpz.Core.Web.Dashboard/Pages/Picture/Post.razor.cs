using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Picture
{
    public partial class Post
    {
        [Inject] private IPictureService PictureService { get; set; }

        [Inject] private ISnackbar Snackbar { get; set; }

        [Inject] private NavigationManager Navigation { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        private bool _isPosting = false;

        private PostPicture _picture = new();

        private List<string> _tags = new();

        //private readonly string[] _inputTags = new string[6];

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
            if (_picture.Image == null || !_picture.Image.ContentType.StartsWith("image/"))
            {
                Snackbar.Add("请选择图片", Severity.Warning);
                return;
            }

            StateHasChanged();
            _isPosting = true;
            using var content = new MultipartFormDataContent();

            var fileContent =
                new StreamContent(_picture.Image.OpenReadStream(AppTools.MaxFileSize));
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(_picture.Image.ContentType);
            content.Add(
                content: fileContent,
                name: "\"image\"",
                fileName: _picture.Image.Name);

            // 处理标签
            var tags = _picture.AdditionsTags?.Split(",")?.Select(x => x?.Trim())?.Where(x => !string.IsNullOrEmpty(x))
                ?.ToList() ?? new List<string>();
            if (_picture.Tags?.Any() == true)
                tags.AddRange(_picture.Tags);
            foreach (var tag in tags.GroupBy(x => x).Select(x => x.Key))
            {
                var tagContent = new StringContent(tag);
                tagContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"tags\""
                };
                content.Add(
                    content: tagContent
                );
            }

            var descContent = new StringContent(_picture.Description ?? "");
            content.Add(
                content: descContent,
                name: "\"description\"");


            await PictureService.UploadAsync(content);
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
            _selectedFiles = new() {{imageFile.Name, imageFile.Size}};
            _picture.Image = imageFile;

            var resizedImage =
                await imageFile.RequestImageFileAsync(imageFile.ContentType, 1000, 1000);
            var jsImageStream = resizedImage.OpenReadStream(AppTools.MaxFileSize);
            var dotnetImageStream = new DotNetStreamReference(jsImageStream);
            await JsRuntime.InvokeVoidAsync("setImageUsingStreaming",
                "imagePreview", dotnetImageStream);
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
}