using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.Timeline;

public partial class Post(
    ITimelineService timelineService,
    NavigationManager navigation,
    IAppDialogService dialogService
) : ComponentBase
{
    private TimelineCreateRequest _timeline = new();
    private bool _isPublishing;
    private MarkdownEditor? _editor;

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
            await timelineService.CreateTimelineAsync(_timeline);
            dialogService.Toast("发布成功", ToastType.Success);
            navigation.NavigateTo("/timeline");
        }
        catch (Exception ex)
        {
            dialogService.Toast($"发布失败：{ex.Message}", ToastType.Error);
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
