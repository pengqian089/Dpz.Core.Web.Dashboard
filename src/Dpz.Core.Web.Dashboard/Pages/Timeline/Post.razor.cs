using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Timeline;

public partial class Post : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Inject]
    private ITimelineService TimelineService { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    private TimelineCreateRequest _timeline = new();

    private bool _isPublishing = false;

    private MarkdownEditor? _editor;

    private async Task PostDataAsync(EditContext context)
    {
        _timeline.Content = await _editor.GetValueAsync();

        _isPublishing = true;
        StateHasChanged();
        await TimelineService.CreateTimelineAsync(_timeline);
        Navigation.NavigateTo("/timeline");
    }

    private async Task<string> UploadPicture(MultipartFormDataContent arg)
    {
        return await TimelineService.UploadAsync(arg);
    }
}
