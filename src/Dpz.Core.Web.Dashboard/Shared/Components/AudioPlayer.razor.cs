using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class AudioPlayer(IJSRuntime jsRuntime) : ComponentBase, IAsyncDisposable
{
    [Parameter]
    [EditorRequired]
    public required string Src { get; set; }

    private ElementReference _audioElement;
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsPlayer;
    private DotNetObjectReference<AudioPlayer>? _objRef;

    private bool _isPlaying;
    private double _currentTime;
    private double _duration;

    private double DisplayTime
    {
        get => _currentTime;
        set => _currentTime = value;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _objRef = DotNetObjectReference.Create(this);
                _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Shared/Components/AudioPlayer.razor.js"
                );
                _jsPlayer = await _jsModule.InvokeAsync<IJSObjectReference>(
                    "init",
                    _objRef,
                    _audioElement
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AudioPlayer init failed: {ex.Message}");
            }
        }
    }

    private async Task TogglePlay()
    {
        if (_jsPlayer == null)
        {
            return;
        }

        if (_isPlaying)
        {
            await _jsPlayer.InvokeVoidAsync("pause");
            _isPlaying = false;
        }
        else
        {
            await _jsPlayer.InvokeVoidAsync("play");
            _isPlaying = true;
        }
    }

    private async Task OnSeekChange(ChangeEventArgs e)
    {
        if (_jsPlayer == null || !double.TryParse(e.Value?.ToString(), out var time))
        {
            return;
        }

        _currentTime = time;
        await _jsPlayer.InvokeVoidAsync("setCurrentTime", time);
    }

    private void OnSeekInput(ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out var time))
        {
            _currentTime = time;
        }
    }

    [JSInvokable]
    public void OnTimeUpdate(double time)
    {
        _currentTime = time;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnDurationChange(double duration)
    {
        _duration = duration;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnEnded()
    {
        _isPlaying = false;
        _currentTime = 0;
        StateHasChanged();
    }

    private string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_jsPlayer != null)
            {
                await _jsPlayer.InvokeVoidAsync("dispose");
                await _jsPlayer.DisposeAsync();
            }
            if (_jsModule != null)
            {
                await _jsModule.DisposeAsync();
            }
            _objRef?.Dispose();
        }
        catch
        {
            // ignored
        }
    }
}
