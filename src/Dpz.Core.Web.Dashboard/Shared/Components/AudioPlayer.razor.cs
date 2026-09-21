using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class AudioPlayer(IJSRuntime jsRuntime, IAppDialogService dialogService)
    : ComponentBase,
        IAsyncDisposable
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
            try
            {
                await _jsPlayer.InvokeVoidAsync("pause");
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions { Title = "错误", Message = $"暂停音频失败：{ex.Message}" }
                );
            }
            _isPlaying = false;
        }
        else
        {
            try
            {
                await _jsPlayer.InvokeVoidAsync("play");
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions { Title = "错误", Message = $"播放音频失败：{ex.Message}" }
                );
            }
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
        try
        {
            await _jsPlayer.InvokeVoidAsync("setCurrentTime", time);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync(
                new AppDialogOptions { Title = "错误", Message = $"跳转播放位置失败：{ex.Message}" }
            );
        }
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
