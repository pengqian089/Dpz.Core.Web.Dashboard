using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Video(IVideoService videoService, IAppDialogService dialogService)
    : ComponentBase
{
    private List<VideoModel> _source = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadVideosAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadVideosAsync()
    {
        _isLoading = true;
        try
        {
            _source = await videoService.GetVideosAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", Models.Dialog.ToastType.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task PreviewVideo(string url, string? title)
    {
        await dialogService.ShowComponentAsync(
            title ?? "视频预览",
            builder =>
            {
                builder.OpenComponent<Player>(0);
                builder.AddAttribute(1, nameof(Player.VideoUrl), url);
                builder.CloseComponent();
            },
            width: "900px"
        );
    }

    private async Task EditVideo(string id)
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "编辑视频信息",
            builder =>
            {
                builder.OpenComponent<Edit>(0);
                builder.AddAttribute(1, nameof(Edit.Id), id);
                builder.CloseComponent();
            },
            width: "600px"
        );

        if (result)
        {
            await LoadVideosAsync();
        }
    }

    private async Task ShowSettingCover(string id)
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "设置缩略图",
            builder =>
            {
                builder.OpenComponent<Screenshot>(0);
                builder.AddAttribute(1, nameof(Screenshot.Id), id);
                builder.CloseComponent();
            },
            width: "500px"
        );

        if (result)
        {
            await LoadVideosAsync();
        }
    }
}
