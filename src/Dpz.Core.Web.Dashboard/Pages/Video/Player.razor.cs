using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

#nullable enable

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Player(
    IJSRuntime jsRuntime) : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public required string VideoUrl { get; set; }

    private readonly string _videoId = $"video-player-{Guid.NewGuid():N}";
    private IJSObjectReference? _module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Pages/Video/Player.razor.js"
            );

            await _module.InvokeVoidAsync("initVideoPlayer", _videoId, VideoUrl);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }
}
