using System;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages;

[Authorize]
public partial class Index(IJSRuntime jsRuntime, ICommunityService communityService)
    : IAsyncDisposable
{
    private bool _isLoading;
    private SummaryInformation? _summary;
    private DateTime _lastUpdated = DateTime.MinValue;
    private IJSObjectReference? _module;

    private int TodayAccessTotal => _summary?.TodayAccessNumber?.Sum(x => x.Count) ?? 0;
    private int WeekAccessTotal => _summary?.WeekAccessNumber?.Sum(x => x.Count) ?? 0;
    private int WeekAverage =>
        _summary?.WeekAccessNumber?.Count > 0
            ? (int)Math.Round((double)WeekAccessTotal / _summary.WeekAccessNumber.Count)
            : 0;
    private int BannerCount => _summary?.Banner?.Count ?? 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryAsync();
        await base.OnInitializedAsync();
    }

    private async Task RefreshSummaryAsync()
    {
        await LoadSummaryAsync();
    }

    private async Task LoadSummaryAsync()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            _summary = await communityService.GetSummaryAsync();
            _lastUpdated = DateTime.Now;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _summary = null;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Don't await Prism highlight if it fails, just run it
        try
        {
            await jsRuntime.InvokeVoidAsync("Prism.highlightAll");
        }
        catch { }

        // Import module if needed
        if (_module == null)
        {
            try
            {
                // Note: Path is relative to the base href or root, using collocated JS convention
                _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Pages/Index.razor.js"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to import Index.razor.js: {ex.Message}");
            }
        }

        if (_summary != null && _module != null)
        {
            // Init Chart
            if (_summary.WeekAccessNumber != null && _summary.WeekAccessNumber.Any())
            {
                var labels = _summary.WeekAccessNumber.Select(x => x.Date).ToArray();
                var data = _summary.WeekAccessNumber.Select(x => x.Count).ToArray();
                try
                {
                    await _module.InvokeVoidAsync("initChart", "visitorChart", labels, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Init Chart failed: {ex.Message}");
                }
            }

            // Init Carousel
            if (_summary.Banner != null && _summary.Banner.Any())
            {
                try
                {
                    await _module.InvokeVoidAsync("initCarousel", "bannerCarousel");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Init Carousel failed: {ex.Message}");
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException) { } // Ignore
            catch (Exception ex)
            {
                Console.WriteLine($"Dispose dashboard module error: {ex}");
            }
        }
    }

    private static string FormatNumber(int value) => value.ToString("N0");
}
