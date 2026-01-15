using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.Timeline;

public partial class Edit(
    ITimelineService timelineService,
    NavigationManager navigation,
    IAppDialogService dialogService
) : ComponentBase
{
    [Parameter]
    public string Id { get; set; } = "";

    private TimelineEditRequest _timeline = new();

    private bool _isPublishing;
    private bool _isLoading;

    private MarkdownEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            var timelineModel = await timelineService.GetTimelineAsync(Id);
            if (timelineModel == null)
            {
                dialogService.Toast("时间轴不存在", ToastType.Error);
                navigation.NavigateTo("/timeline");
                return;
            }

            _timeline = new TimelineEditRequest
            {
                Id = timelineModel.Id,
                Content = timelineModel.Content,
                Date = timelineModel.Date,
                More = timelineModel.More,
                Title = timelineModel.Title,
            };
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", ToastType.Error);
            navigation.NavigateTo("/timeline");
            return;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task PostDataAsync(EditContext context)
    {
        if (_editor == null)
        {
            dialogService.Toast("编辑器未初始化", ToastType.Error);
            return;
        }

        _timeline.Content = await _editor.GetValueAsync();
        if (string.IsNullOrWhiteSpace(_timeline.Content))
        {
            dialogService.Toast("请输入内容", ToastType.Warning);
            return;
        }

        _isPublishing = true;
        StateHasChanged();
        try
        {
            await timelineService.EditTimelineAsync(_timeline);
            dialogService.Toast("保存成功", ToastType.Success);
            navigation.NavigateTo("/timeline");
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isPublishing = false;
            StateHasChanged();
        }
    }

    private void BackToList()
    {
        navigation.NavigateTo("/timeline");
    }
}
