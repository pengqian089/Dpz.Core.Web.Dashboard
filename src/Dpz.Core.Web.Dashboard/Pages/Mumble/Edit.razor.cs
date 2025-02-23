using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Mumble
{
    public partial class Edit
    {
        [Parameter] public string Id { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        [Inject] private IMumbleService MumbleService { get; set; }

        [Inject] private ISnackbar Snackbar { get; set; }

        [Inject] private NavigationManager Navigation { get; set; }

        private readonly object _t = new();

        private bool _isLoading = false;

        private bool _isPublishing = false;

        private MumbleModel _model = new();
        private MarkdownEditor _editor;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            _model = await MumbleService.GetMumbleAsync(Id);
            _isLoading = false;
            await base.OnInitializedAsync();
        }

        private async Task PostDataAsync(EditContext context)
        {
            if (string.IsNullOrEmpty(_model.Id))
            {
                Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
                Snackbar.Configuration.MaxDisplayedSnackbars = 10;
                Snackbar.Add("缺少参数", Severity.Error);
                return;
            }

            var markdown = await _editor.GetValueAsync();
            var content = Markdown.ToHtml(markdown);
            if (string.IsNullOrEmpty(markdown) || string.IsNullOrEmpty(content))
            {
                Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
                Snackbar.Configuration.MaxDisplayedSnackbars = 10;
                Snackbar.Add("请输入内容", Severity.Warning);
                return;
            }

            _isPublishing = true;
            StateHasChanged();
            await MumbleService.EditAsync(_model.Id, markdown, content);
            await _editor.DisposeAsync();
            Navigation.NavigateTo("/mumble");
        }

        private async Task<string> UploadAsync(MultipartFormDataContent arg)
        {
            return await MumbleService.UploadAsync(arg);
        }
    }
}