using System;
using System.Timers;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Shared;

public partial class AuthLoginPrompt(NavigationManager navigation) : IDisposable
{
    [Parameter]
    public string? ReturnUrl { get; set; }

    [Parameter]
    public int Seconds { get; set; } = 5;

    [Parameter]
    public string Message { get; set; } = "需要登录后访问此页面";

    [Parameter]
    public string Description { get; set; } = "请先完成身份验证，然后我们会带您返回当前页面";

    private int _seconds;
    private bool _autoRedirect = true;
    private Timer? _timer;

    protected override void OnInitialized()
    {
        _seconds = Seconds;
        StartTimer();
    }

    private void StartTimer()
    {
        _timer?.Dispose();
        if (!_autoRedirect)
        {
            return;
        }

        _timer = new Timer(1000);
        _timer.Elapsed += (_, _) =>
        {
            if (!_autoRedirect)
            {
                _timer?.Stop();
                return;
            }

            if (_seconds <= 1)
            {
                _timer.Stop();
                InvokeAsync(LoginNow);
                return;
            }

            _seconds--;
            InvokeAsync(StateHasChanged);
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void CancelAuto()
    {
        _autoRedirect = false;
        _timer?.Stop();
        StateHasChanged();
    }

    private void LoginNow()
    {
        var url = $"/authentication/login?returnUrl={Uri.EscapeDataString(ReturnUrl ?? navigation.Uri)}";
        navigation.NavigateTo(url, forceLoad: false);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
