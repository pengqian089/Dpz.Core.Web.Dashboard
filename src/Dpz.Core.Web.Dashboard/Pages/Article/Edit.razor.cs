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
    public partial class Edit
    {
        [Parameter] public string Id { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        [Inject] private IArticleService ArticleService { get; set; }

        [Inject] private ISnackbar Snackbar { get; set; }

        [Inject] private NavigationManager Navigation { get; set; }

        private List<string> _tags = new();

        private string _addTag = "";

        private ArticleEditRequest _article = new();

        private bool _isLoading = true;

        private bool _isPublishing;

        private bool _isEditorInit;

        MarkdownEditor _editor;
        protected override async Task OnInitializedAsync()
        {
            _tags = await ArticleService.GetTagsAsync();
            
            _isLoading = true;
            var article = await ArticleService.GetArticleAsync(Id);
            if (article != null)
            {
                _article = new ArticleEditRequest
                {
                    Id = article.Id,
                    Content = article.HtmlContent,
                    Introduction = article.Introduction,
                    Markdown = article.Markdown,
                    Tags = article.Tags,
                    Title = article.Title
                };
            }
            _isLoading = false;
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }

        private async Task PublishArticleAsync(EditContext context)
        {
            if (string.IsNullOrEmpty(_article.Id))
            {
                Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
                Snackbar.Configuration.MaxDisplayedSnackbars = 10;
                Snackbar.Add("缺少参数", Severity.Error);
                return;
            }

            
            _article.Markdown = await _editor.GetValueAsync();
            _article.Content = Markdig.Markdown.ToHtml(_article.Markdown);
            
            if (string.IsNullOrEmpty(_article.Content) || string.IsNullOrEmpty(_article.Markdown))
            {
                Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
                Snackbar.Configuration.MaxDisplayedSnackbars = 10;
                Snackbar.Add("请输入内容", Severity.Warning);
                return;
            }


            _isPublishing = true;
            //await Task.Delay(1000);
            //_isPublishing = false;

            var tags = _article.Tags.ToList();

            if (!string.IsNullOrEmpty(_addTag))
                tags.Add(_addTag);
            _article.Tags = tags;
            StateHasChanged();
            await ArticleService.EditAsync(_article);
            await _editor.DisposeAsync();
            Navigation.NavigateTo("/article");
            //Console.WriteLine(JsonSerializer.Serialize(_article, new JsonSerializerOptions {WriteIndented = true}));
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

        private async Task<string> UploadPicture(MultipartFormDataContent content)
        {
            return await ArticleService.UploadAsync(content);
        }
    }
}