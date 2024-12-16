using System.IO;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.DynamicPage
{
    public partial class Post
    {

        [Inject] private IDynamicPageService DynamicPageService { get; set; }

        [Inject] private ISnackbar Snackbar { get; set; }

        [Inject] private NavigationManager Navigation { get; set; }

        private string _name = "";

        private readonly StringWriter _htmlContent = new();

        private readonly object _t = new();

        private bool _isPublishing = false;

        private HtmlEditor _editor;

        protected override async Task OnInitializedAsync()
        {
            var htmlContext = BrowsingContext.New(Configuration.Default);
            var html = await htmlContext.OpenAsync(x => x.Content("<!DOCTYPE html>"));
            
            html.DocumentElement.SetAttribute("lang", "zh-Hans");
            html.Title = "新建动态页";
            var h1 = html.CreateElement("h1");
            h1.TextContent = "新建动态页";
            html.Body?.Append(h1);
            html.ToHtml(_htmlContent, new PrettyMarkupFormatter());
            await base.OnInitializedAsync();
        }

        private async Task PostDataAsync(EditContext context)
        {
            var content = await _editor.GetValueAsync();
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;
            if (string.IsNullOrEmpty(_name))
            {
                Snackbar.Add("请输入页面名称", Severity.Warning);
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                Snackbar.Add("请输入内容", Severity.Warning);
                return;
            }
            _isPublishing = true;
            if (await DynamicPageService.ExistsAsync(_name))
            {
                Snackbar.Add("该页面名称已存在", Severity.Warning);
                _isPublishing = false;
                return;
            }
            StateHasChanged();
            await DynamicPageService.CreateDynamicPage(_name, content);
            await _editor.DisposeAsync();
            // bug 在回车提交时，返回列表不加载数据
            Navigation.NavigateTo("/dynamic");
        }
    }
}