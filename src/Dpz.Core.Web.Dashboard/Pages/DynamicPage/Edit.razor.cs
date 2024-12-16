using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html;
using BlazorMonaco.Editor;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.DynamicPage
{
    public partial class Edit
    {
        [Parameter]public string Id { get; set; }

        [Inject]private IJSRuntime JsRuntime { get; set; }

        [Inject]private IDynamicPageService DynamicPageService { get; set; }

        [Inject]private ISnackbar Snackbar { get; set; }

        [Inject]private NavigationManager Navigation { get; set; }

        private string _name = "";

        private StringWriter _htmlContent = new();

        private readonly object _t = new();

        private bool _isPublishing = false;

        private bool _isLoading = false;

        private bool _isEditorInit = false;

        private DynamicPageModel _model = new();

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            _model = await DynamicPageService.GetDynamicPageAsync(Id);
            var htmlContext = BrowsingContext.New(Configuration.Default);
            var html = await htmlContext.OpenAsync(x => x.Content(_model.HtmlContent));
            html.ToHtml(_htmlContent, new PrettyMarkupFormatter());
            _isLoading = false;
            await base.OnInitializedAsync();
            StateHasChanged();
        }

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
        private HtmlEditor  _editor;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // 不知道为什么会出现并发，导致编辑器被初始化多次
            // await SemaphoreSlim.WaitAsync();
            // try
            // {
            //     if (!_isEditorInit && await JsRuntime.InvokeAsync<bool>("isExistsDynamicEditor"))
            //     {
            //         await JsRuntime.InvokeVoidAsync("initDynamicEditor");
            //         _isEditorInit = true;
            //         await base.OnAfterRenderAsync(firstRender);
            //     }
            // }
            // finally
            // {
            //     SemaphoreSlim.Release();
            // }
            await base.OnAfterRenderAsync(firstRender);
        }
        
        private async Task PostDataAsync(EditContext context)
        {
            var content = await _editor.GetValueAsync();
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;

            if (string.IsNullOrEmpty(_model.Id))
            {
                Snackbar.Add("参数错误", Severity.Warning);
                return;
            }
            
            if (string.IsNullOrEmpty(content))
            {
                Snackbar.Add("请输入内容", Severity.Warning);
                return;
            }
            _isPublishing = true;
            StateHasChanged();
            await DynamicPageService.EditDynamicPage(_model.Id, content);
            await _editor.DisposeAsync();
            Navigation.NavigateTo("/dynamic");
        }
    }
}