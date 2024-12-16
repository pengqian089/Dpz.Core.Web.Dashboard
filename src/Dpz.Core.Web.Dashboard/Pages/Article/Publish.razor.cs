using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Article
{
    public partial class Publish
    {
        [Inject]private IJSRuntime JsRuntime { get; set; }

        [Inject]private IArticleService ArticleService { get; set; }

        [Inject]private ISnackbar Snackbar { get; set; }

        [Inject]private NavigationManager Navigation { get; set; }

        private List<string> _tags = new();

        private string _addTag = "";

        private ArticlePublishRequest _article = new();

        private bool _isPublishing = false;
        private MarkdownEditor _editor;

        protected override async Task OnInitializedAsync()
        {
            _tags = await ArticleService.GetTagsAsync();
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task PublishArticleAsync(EditContext context)
        {
            _article.Markdown = await _editor.GetValueAsync();
            _article.Content = Markdig.Markdown.ToHtml(_article.Markdown);
            
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;
            if (string.IsNullOrEmpty(_article.Content) || string.IsNullOrEmpty(_article.Markdown))
            {
               Snackbar.Add("请输入内容", Severity.Warning);
                return;
            }

            _isPublishing = true;
            if (await ArticleService.ExistsAsync(_article.Title))
            {
                _isPublishing = false;
                Snackbar.Add("该标题已经存在", Severity.Warning);
                return;
            }

            var tags = _article.Tags.ToList();
            if (!string.IsNullOrEmpty(_addTag))
                tags.Add(_addTag);
            _article.Tags = tags;
            StateHasChanged();
            await ArticleService.PublishAsync(_article);
            await _editor.DisposeAsync();
            Navigation.NavigateTo("/article");
        }

        private async Task<IEnumerable<string>> SearchTagAsync(string value)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrEmpty(value))
                    return _tags;
                return _tags.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
            });
        }

        private async Task<string> UploadPicture(MultipartFormDataContent arg)
        {
            return await ArticleService.UploadAsync(arg);
        }
    }
}