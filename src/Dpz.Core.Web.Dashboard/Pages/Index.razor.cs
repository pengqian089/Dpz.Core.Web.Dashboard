using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages;

[Authorize]
public partial class Index(
    IJSRuntime jsRuntime,
    ICommunityService communityService,
    IAssetManifestService assetManifestService,
    IAppDialogService dialogService,
    ILogger<Index> logger
) : IAsyncDisposable
{
    private readonly CancellationTokenSource _disposeTokenSource = new();
    private bool _chartsNeedRender;
    private bool _isLoading = true;
    private bool _isRefreshing;
    private bool _loadFailed;
    private int _renderVersion;
    private int _renderedVersion = -1;
    private int _logHighlightedVersion = -1;
    private string? _errorMessage;
    private SummaryInformation? _summary;
    private DateTime? _lastUpdated;
    private IJSObjectReference? _module;
    private IJSObjectReference? _markdownPreviewModule;

    private int TodayAccessTotal => _summary?.TodayAccessNumber.Sum(x => x.Count) ?? 0;

    private int WeekAccessTotal => _summary?.WeekAccessNumber.Sum(x => x.Count) ?? 0;

    private int WeekAverage =>
        _summary?.WeekAccessNumber.Count > 0
            ? (int)Math.Round((double)WeekAccessTotal / _summary.WeekAccessNumber.Count)
            : 0;

    private int ArticleTotalCount => _summary?.ArticleTotalCount ?? 0;

    private int TodayArticleCount => _summary?.TodayArticleCount ?? 0;

    private int BannerCount => _summary?.Banner.Count ?? 0;

    private int PopularPageTotal => PopularPages.Sum(x => x.Count);

    private int RefererTotal => Referers.Sum(x => x.Count);

    private int PopularPagePeak => PopularPages.FirstOrDefault()?.Count ?? 0;

    private double SlowestRequestElapsed => SlowRequests.FirstOrDefault()?.Elapsed ?? 0;

    private DateTime? SummaryUpdateTime =>
        _summary?.UpdateTime is { } updateTime && updateTime != default ? updateTime : _lastUpdated;

    private string LastUpdatedText =>
        SummaryUpdateTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture) ?? "--";

    private IReadOnlyList<PictureResponseModel> VisibleBanners =>
        _summary?.Banner.Where(x => !string.IsNullOrWhiteSpace(x.AccessUrl)).Take(6).ToArray()
        ?? [];

    private IReadOnlyList<ArticleMiniResponse> LatestArticles =>
        _summary?.LatestArticles.Take(6).ToArray() ?? [];

    private IReadOnlyList<RequestPathSummary> PopularPages =>
        _summary?.AccessLogStatistics.PopularPages.OrderByDescending(x => x.Count).Take(6).ToArray()
        ?? [];

    private IReadOnlyList<SlowRequestSummary> SlowRequests =>
        _summary
            ?.AccessLogStatistics.SlowRequests.OrderByDescending(x => x.Elapsed)
            .Take(6)
            .ToArray()
        ?? [];

    private IReadOnlyList<RefererSummary> Referers =>
        _summary?.AccessLogStatistics.Referers.OrderByDescending(x => x.Count).Take(5).ToArray()
        ?? [];

    private IReadOnlyList<BrowserUsageSummary> BrowserUsage =>
        _summary?.AccessLogStatistics.BrowserUsage.OrderByDescending(x => x.Count).Take(6).ToArray()
        ?? [];

    private IReadOnlyList<HourlyTrafficSummary> HourlyTraffic =>
        _summary
            ?.AccessLogStatistics.HourlyTraffic.OrderBy(x => x.Date)
            .ThenBy(x => x.Hour)
            .TakeLast(24)
            .ToArray()
        ?? [];

    private int BrowserTotal => BrowserUsage.Sum(x => x.Count);

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryAsync(refreshCache: false);
    }

    private async Task RefreshSummaryAsync()
    {
        await LoadSummaryAsync(refreshCache: true);
    }

    private async Task RetryLoadAsync()
    {
        await LoadSummaryAsync(refreshCache: false);
    }

    private async Task LoadSummaryAsync(bool refreshCache)
    {
        if (refreshCache)
        {
            _isRefreshing = true;
        }
        else
        {
            _isLoading = true;
            _loadFailed = false;
        }

        _errorMessage = null;
        StateHasChanged();

        try
        {
            var cancellationToken = _disposeTokenSource.Token;
            var summary = refreshCache
                ? await communityService.RefreshSummaryCacheAsync(cancellationToken)
                : await communityService.GetSummaryAsync(cancellationToken);

            if (summary == null)
            {
                throw new InvalidOperationException("没有收到汇总信息。");
            }

            _summary = summary;
            _lastUpdated = DateTime.Now;
            _loadFailed = false;
            _chartsNeedRender = true;
            _renderVersion++;

            if (refreshCache)
            {
                dialogService.ShowToast(
                    new AppToastOptions
                    {
                        Message = "汇总缓存已刷新",
                        Level = AppFeedbackLevel.Success,
                    }
                );
            }
        }
        catch (OperationCanceledException) when (_disposeTokenSource.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "failed to load dashboard summary. Refresh cache: {RefreshCache}",
                refreshCache
            );

            _errorMessage = ex.Message;
            _loadFailed = _summary == null;
            dialogService.ShowToast(
                new AppToastOptions
                {
                    Message = refreshCache ? "刷新汇总缓存失败" : "首页数据加载失败",
                    Level = AppFeedbackLevel.Danger,
                }
            );
        }
        finally
        {
            if (refreshCache)
            {
                _isRefreshing = false;
            }
            else
            {
                _isLoading = false;
            }

            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_summary == null)
        {
            return;
        }

        if (_chartsNeedRender && _renderedVersion != _renderVersion)
        {
            await EnsureDashboardModuleAsync();
            await RenderChartsAsync();
        }

        if (
            _logHighlightedVersion != _renderVersion
            && !string.IsNullOrWhiteSpace(_summary.LatestLogs)
        )
        {
            await HighlightLogsAsync();
        }
    }

    private async Task EnsureDashboardModuleAsync()
    {
        if (_module != null)
        {
            return;
        }

        try
        {
            var modulePath = await assetManifestService.GetAssetPathAsync("src/pages/dashboard.ts");
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                _disposeTokenSource.Token,
                modulePath
            );
        }
        catch (OperationCanceledException) when (_disposeTokenSource.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to import dashboard module");
        }
    }

    private async Task EnsureMarkdownPreviewModuleAsync()
    {
        if (_markdownPreviewModule != null)
        {
            return;
        }

        try
        {
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/markdown-preview.ts"
            );
            _markdownPreviewModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                _disposeTokenSource.Token,
                modulePath
            );
        }
        catch (OperationCanceledException) when (_disposeTokenSource.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to import markdown preview module");
        }
    }

    private async Task RenderChartsAsync()
    {
        if (_module == null || _summary == null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync(
                "initLineChart",
                _disposeTokenSource.Token,
                "visitorTrendChart",
                _summary.WeekAccessNumber.Select(x => FormatAccessLabel(x.Date)).ToArray(),
                _summary.WeekAccessNumber.Select(x => x.Count).ToArray()
            );

            await _module.InvokeVoidAsync(
                "initBarChart",
                _disposeTokenSource.Token,
                "hourlyTrafficChart",
                HourlyTraffic.Select(FormatHourlyLabel).ToArray(),
                HourlyTraffic.Select(x => x.Count).ToArray()
            );

            await _module.InvokeVoidAsync(
                "initDoughnutChart",
                _disposeTokenSource.Token,
                "browserUsageChart",
                BrowserUsage
                    .Select(x => string.IsNullOrWhiteSpace(x.Browser) ? "未知" : x.Browser)
                    .ToArray(),
                BrowserUsage.Select(x => x.Count).ToArray()
            );

            await _module.InvokeVoidAsync(
                "initCarousel",
                _disposeTokenSource.Token,
                "dashboardBannerCarousel"
            );

            _chartsNeedRender = false;
            _renderedVersion = _renderVersion;
        }
        catch (OperationCanceledException) when (_disposeTokenSource.IsCancellationRequested) { }
        catch (JSDisconnectedException) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to render dashboard charts");
        }
    }

    private async Task HighlightLogsAsync()
    {
        await EnsureMarkdownPreviewModuleAsync();

        if (_markdownPreviewModule == null)
        {
            return;
        }

        try
        {
            await _markdownPreviewModule.InvokeVoidAsync(
                "highlightAll",
                _disposeTokenSource.Token,
                true
            );
            _logHighlightedVersion = _renderVersion;
        }
        catch (OperationCanceledException) when (_disposeTokenSource.IsCancellationRequested) { }
        catch (JSDisconnectedException) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to highlight dashboard logs");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposeTokenSource.Cancel();

        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to dispose dashboard module");
            }
        }

        if (_markdownPreviewModule != null)
        {
            try
            {
                await _markdownPreviewModule.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to dispose markdown preview module");
            }
        }

        _disposeTokenSource.Dispose();
    }

    private static string FormatNumber(int value) =>
        value.ToString("N0", CultureInfo.CurrentCulture);

    private static string FormatElapsed(double value)
    {
        return value >= 1000 ? $"{value / 1000:N1}s" : $"{value:N0}ms";
    }

    private static string FormatPercentage(BrowserUsageSummary item, int total)
    {
        var percentage = item.Percentage;
        if (percentage <= 0 && total > 0)
        {
            percentage = (decimal)item.Count / total * 100;
        }

        return $"{percentage:0.#}%";
    }

    private static string FormatCssPercent(int value, int total)
    {
        if (value <= 0 || total <= 0)
        {
            return "0%";
        }

        var percentage = Math.Min(100, (double)value / total * 100);
        return FormattableString.Invariant($"{percentage:0.##}%");
    }

    private static string FormatAccessLabel(string value)
    {
        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, out var date))
        {
            return date.ToString("MM-dd", CultureInfo.CurrentCulture);
        }

        return string.IsNullOrWhiteSpace(value) ? "--" : value;
    }

    private static string FormatHourlyLabel(HourlyTrafficSummary item)
    {
        var hour = $"{item.Hour:00}:00";
        if (DateTime.TryParse(item.Date, CultureInfo.CurrentCulture, out var date))
        {
            return $"{date:MM-dd} {hour}";
        }

        return hour;
    }

    private static string FormatArticleDate(DateTime value)
    {
        return value == default ? "--" : value.ToString("MM-dd HH:mm", CultureInfo.CurrentCulture);
    }

    private static string FormatSlowRequestTime(string? value)
    {
        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, out var date))
        {
            return date.ToString("MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        }

        return string.IsNullOrWhiteSpace(value) ? "--" : value;
    }

    private static string FormatPath(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "/" : value;
    }

    private static string FormatReferer(string? value)
    {
        var normalizedValue = NormalizeRefererText(value);
        if (string.IsNullOrWhiteSpace(normalizedValue) || normalizedValue == "-")
        {
            return "直接访问";
        }

        if (Uri.TryCreate(normalizedValue, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        return normalizedValue;
    }

    private static string FormatRefererAddress(string? value)
    {
        var normalizedValue = NormalizeRefererText(value);
        return string.IsNullOrWhiteSpace(normalizedValue) ? "-" : normalizedValue;
    }

    private static string NormalizeRefererText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var text = value.Trim();
        if (text.Equals("null", StringComparison.OrdinalIgnoreCase) || text == "[]")
        {
            return "";
        }

        if (TryReadFirstArrayItem(text, out var arrayValue))
        {
            return NormalizeRefererText(arrayValue);
        }

        if (text.Length >= 2 && text[0] == text[^1] && text[0] is '\'' or '"')
        {
            text = text[1..^1].Trim();
        }

        return text.Equals("null", StringComparison.OrdinalIgnoreCase) ? "" : text;
    }

    private static bool TryReadFirstArrayItem(string value, out string? item)
    {
        item = null;

        if (value.Length < 2 || value[0] != '[' || value[^1] != ']')
        {
            return false;
        }

        var inner = value[1..^1].Trim();
        if (string.IsNullOrWhiteSpace(inner))
        {
            return true;
        }

        if (inner[0] is '\'' or '"')
        {
            var quote = inner[0];
            for (var i = 1; i < inner.Length; i++)
            {
                if (inner[i] == quote && inner[i - 1] != '\\')
                {
                    item = inner[1..i];
                    return true;
                }
            }

            item = inner.Trim(quote).Trim();
            return true;
        }

        var separatorIndex = inner.IndexOf(',');
        item = separatorIndex >= 0 ? inner[..separatorIndex].Trim() : inner;
        return true;
    }

    private static string GetStatusClass(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => "dashboard-status dashboard-status--danger",
            >= 400 => "dashboard-status dashboard-status--warning",
            >= 300 => "dashboard-status dashboard-status--info",
            _ => "dashboard-status dashboard-status--success",
        };
    }

    private static string GetArticleImage(ArticleMiniResponse article)
    {
        if (!string.IsNullOrWhiteSpace(article.MainImage))
        {
            return article.MainImage;
        }

        if (!string.IsNullOrWhiteSpace(article.MainImageMetadata?.Url))
        {
            return article.MainImageMetadata.Value.Url;
        }

        return article.ImagesAddress.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "";
    }

    private static string GetArticleAuthor(ArticleMiniResponse article)
    {
        return string.IsNullOrWhiteSpace(article.Author?.Name) ? "未知作者" : article.Author.Name;
    }

    private static string GetBrowserColor(int index)
    {
        var colors = new[] { "#60a5fa", "#34d399", "#fbbf24", "#f472b6", "#a78bfa", "#22d3ee" };

        return colors[index % colors.Length];
    }
}
