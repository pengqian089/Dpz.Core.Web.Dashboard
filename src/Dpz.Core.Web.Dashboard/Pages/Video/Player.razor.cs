using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

#nullable enable

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Player(IJSRuntime jsRuntime, IAssetManifestService assetManifestService)
    : ComponentBase,
        IAsyncDisposable
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
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);

            await _module.InvokeVoidAsync("initVideoPlayer", _videoId, VideoUrl);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("disposeVideoPlayer", _videoId);
            await _module.DisposeAsync();
        }
    }
}
