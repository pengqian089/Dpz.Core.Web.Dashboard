using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Music;

public partial class List(
    IMusicService musicService,
    NavigationManager navigation,
    IAppDialogService dialogService
) : ComponentBase
{
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private const int PageSize = 12;
    private string _title = "";
    private List<MusicModel>? _items;
    private bool _isLoading = true;

    private List<MusicModel> Items => _items ?? [];

    protected override async Task OnInitializedAsync()
    {
        ReadQueryParameters();
        await LoadDataAsync();
        await base.OnInitializedAsync();
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
        var baseUri = navigation.ToAbsoluteUri("/music").GetLeftPart(UriPartial.Path);
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

        var list = await musicService.GetPageAsync(_title, _pageIndex, PageSize);
        _items = list.ToList();
        _totalCount = list.TotalItemCount;
        _totalPages = list.TotalPageCount;

        _isLoading = false;
        StateHasChanged();
    }

    private async Task Search()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Search();
        }
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        UpdateUrl();
        await LoadDataAsync();
    }

    private void AddMusic()
    {
        navigation.NavigateTo("/music/add");
    }

    private void ViewDetail(string id)
    {
        navigation.NavigateTo($"/music/detail/{id}");
    }

    private async Task DeleteAsync(string id)
    {
        var ok = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (ok)
        {
            await musicService.DeleteAsync(id);
            await LoadDataAsync();
        }
    }
}
