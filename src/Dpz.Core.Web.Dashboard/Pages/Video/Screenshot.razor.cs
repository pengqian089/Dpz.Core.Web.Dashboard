using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Screenshot(
    IVideoService videoService,
    IAppDialogService dialogService) : ComponentBase
{
    [Parameter]
    public required string Id { get; set; }

    [CascadingParameter]
    public Action<object?>? Close { get; set; }

    private double _seconds = 1;
    private bool _isLoading = true;
    private bool _isSaving;
    private VideoMetaDataModel? _model;
    private string _validationError = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _model = await videoService.GetVideoMetadataAsync(Id);
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

        // 验证时间范围
        _validationError = "";
        if (_seconds < 0.01)
        {
            _validationError = "时间不能小于0.01秒";
            return;
        }
        if (_seconds > _model.Duration)
        {
            _validationError = $"时间不能超过视频时长（{_model.Duration:F2}秒）";
            return;
        }

        _isSaving = true;
        try
        {
            await videoService.SetVideoScreenshotAsync(Id, _seconds);
            dialogService.Toast("设置成功", Models.Dialog.ToastType.Success);
            Close?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"设置失败：{ex.Message}", Models.Dialog.ToastType.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }
}
