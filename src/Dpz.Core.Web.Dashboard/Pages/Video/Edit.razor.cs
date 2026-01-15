using System;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

#nullable enable

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Edit(
    IVideoService videoService,
    IAppDialogService dialogService) : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }

    [CascadingParameter]
    public Action<object?>? Close { get; set; }

    private VideoModel? _model;
    private bool _isLoading = true;
    private bool _isSaving;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var videos = await videoService.GetVideosAsync();
            _model = videos.FirstOrDefault(v => v.Id == Id);
            if (_model == null)
            {
                dialogService.Toast("视频不存在", Models.Dialog.ToastType.Error);
                Cancel();
                return;
            }
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", Models.Dialog.ToastType.Error);
            Cancel();
        }
        finally
        {
            _isLoading = false;
        }

        await base.OnInitializedAsync();
    }

    private void Cancel()
    {
        Close?.Invoke(false);
    }

    private async Task SaveAsync()
    {
        if (_model == null)
        {
            return;
        }

        _isSaving = true;
        try
        {
            _model.Tags = string.IsNullOrWhiteSpace(_model.TagsValue)
                ? []
                : _model.TagsValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            await videoService.SaveVideoInformationAsync(_model);
            dialogService.Toast("保存成功", Models.Dialog.ToastType.Success);
            Close?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", Models.Dialog.ToastType.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }
}
