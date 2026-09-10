using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

#nullable enable

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Player(
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService,
    IAppDialogService dialogService,
    ILogger<Player> logger
) : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public required string VideoUrl { get; set; }

    private readonly string _videoId = $"video-player-{Guid.NewGuid():N}";
    private IJSObjectReference? _module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/pages/video-player.ts"
            );
            try
            {
                _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
                if (_module != null)
                {
                    await _module.InvokeVoidAsync("initVideoPlayer", _videoId, VideoUrl);
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions
                    {
                        Title = "错误",
                        Message = $"视频播放器初始化失败：{ex.Message}",
                    }
                );
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("disposeVideoPlayer", _videoId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispose video player.");
            }
            await _module.DisposeAsync();
        }
    }
}
