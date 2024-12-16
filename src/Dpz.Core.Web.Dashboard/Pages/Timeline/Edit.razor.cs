using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages.Timeline
{
    public partial class Edit
    {
        [Parameter] public string Id { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        [Inject] private ITimelineService TimelineService { get; set; }

        [Inject] private NavigationManager Navigation { get; set; }

        private TimelineEditRequest _timeline = new();

        private bool _isPublishing = false;

        private bool _isLoading = false;

        private bool _isEditorInit = false;
        private MarkdownEditor _editor;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            var timelineModel = await TimelineService.GetTimelineAsync(Id);
            _timeline = new TimelineEditRequest
            {
                Id = timelineModel.Id,
                Content = timelineModel.Content,
                Date = timelineModel.Date,
                More = timelineModel.More,
                Title = timelineModel.Title
            };
            _isLoading = false;
            await base.OnInitializedAsync();
        }

        private async Task PostDataAsync(EditContext context)
        {
            _timeline.Content = await _editor.GetValueAsync();

            _isPublishing = true;
            StateHasChanged();
            await TimelineService.EditTimelineAsync(_timeline);
            await _editor.DisposeAsync();
            Navigation.NavigateTo("/timeline");
        }

        private async Task<string> UploadPicture(MultipartFormDataContent arg)
        {
            return await TimelineService.UploadAsync(arg);
        }
    }
}