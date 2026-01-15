using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Dpz.Core.Web.Dashboard.Shared.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Timeline;

public partial class List(
    ITimelineService timelineService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private const int PageSize = 12;
    private string _title = "";
    private bool _isLoading = true;
    private List<TimelineModel> _timelines = [];

    protected override async Task OnInitializedAsync()
    {
        ReadQueryParameters();
        await LoadDataAsync();
    }

    private void ReadQueryParameters()
    {
        var uri = new Uri(navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (int.TryParse(query["page"], out var page) && page > 0)
        {
            _pageIndex = page;
        }

        _title = query["title"] ?? "";
    }

    private void UpdateUrl()
    {
        var baseUri = navigation.ToAbsoluteUri("/timeline").GetLeftPart(UriPartial.Path);
        var queryParams = new List<string>();

        if (_pageIndex > 1)
        {
            queryParams.Add($"page={_pageIndex}");
        }

        if (!string.IsNullOrWhiteSpace(_title))
        {
            queryParams.Add($"title={Uri.EscapeDataString(_title)}");
        }

        var url = queryParams.Count > 0 ? $"{baseUri}?{string.Join("&", queryParams)}" : baseUri;
        navigation.NavigateTo(url, false);
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        var result = await timelineService.GetPageAsync(_title, _pageIndex, PageSize);
        _timelines = result.ToList();
        _totalCount = result.TotalItemCount;
        _totalPages = result.TotalPageCount;

        _isLoading = false;
        StateHasChanged();
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task Search()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task Reload()
    {
        await LoadDataAsync();
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Search();
        }
    }

    private void CreateTimeline()
    {
        navigation.NavigateTo("/timeline/post");
    }

    private void EditTimeline(string id)
    {
        navigation.NavigateTo($"/timeline/edit/{id}");
    }

    private async Task ViewContent(TimelineModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Content))
        {
            dialogService.Toast("当前内容为空", ToastType.Warning);
            return;
        }

        await dialogService.ShowComponentAsync(
            "时间轴内容",
            BuildContentPreview(model.Content),
            "900px"
        );
    }

    private async Task DeleteAsync(string id)
    {
        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (confirmed)
        {
            await timelineService.DeleteAsync(id);
            dialogService.Toast("删除成功", ToastType.Success);
            await LoadDataAsync();
        }
    }

    private static RenderFragment BuildContentPreview(string content)
    {
        return builder =>
        {
            builder.OpenComponent<MarkdownPreview>(0);
            builder.AddAttribute(1, "Markdown", content);
            builder.AddAttribute(2, "Style", "background: transparent; border: none;");
            builder.CloseComponent();
        };
    }

    private static string BuildLink(string? more)
    {
        if (string.IsNullOrWhiteSpace(more))
        {
            return "";
        }

        return more.StartsWith("/") ? $"{Program.WebHost}{more}" : more;
    }
}
