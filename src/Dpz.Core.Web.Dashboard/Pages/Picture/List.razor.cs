using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Picture;

public partial class List(
    IPictureService pictureService,
    NavigationManager navigation,
    IAppDialogService dialogService
) : ComponentBase
{
    private int _pageIndex = 1;
    private int _totalPages;
    private int _totalCount;
    private const int PageSize = 10;

    private string _tag = "";
    private string _description = "";
    private int _pictureType = -1;
    private string _viewMode = "grid";

    private List<string> _tags = [];
    private List<PictureResponseModel> _pictures = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        ReadQueryParameters();
        _tags = await pictureService.GetTagsAsync();
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

        _tag = query["tag"] ?? "";
        _description = query["description"] ?? "";

        if (int.TryParse(query["type"], out var type))
        {
            _pictureType = type;
        }

        _viewMode = query["view"] ?? "grid";
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        var result = await pictureService.GetPageAsync(
            _tag,
            _description,
            _pictureType,
            _pageIndex,
            PageSize
        );

        _pictures = result.ToList();
        _totalCount = result.TotalItemCount;
        _totalPages = result.TotalPageCount;

        _isLoading = false;
        StateHasChanged();
    }

    private void UpdateUrl()
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = _pageIndex > 1 ? _pageIndex.ToString() : null,
            ["tag"] = !string.IsNullOrEmpty(_tag) ? _tag : null,
            ["description"] = !string.IsNullOrEmpty(_description) ? _description : null,
            ["type"] = _pictureType >= 0 ? _pictureType.ToString() : null,
            ["view"] = _viewMode != "grid" ? _viewMode : null,
        };

        var filteredParams = queryParams
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}");

        var queryString = string.Join("&", filteredParams);
        var newUrl = string.IsNullOrEmpty(queryString) ? "/picture" : $"/picture?{queryString}";

        navigation.NavigateTo(newUrl, forceLoad: false);
    }

    private async Task Search()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task HandlePageChanged(int newPage)
    {
        _pageIndex = newPage;
        UpdateUrl();
        await LoadDataAsync();
    }

    private void ToggleViewMode(string mode)
    {
        _viewMode = mode;
        UpdateUrl();
        StateHasChanged();
    }

    private void PostPicture()
    {
        navigation.NavigateTo("/picture/post");
    }

    private void EditPicture(string id)
    {
        navigation.NavigateTo($"/picture/edit/{id}");
    }

    private async Task DeleteAsync(string id)
    {
        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (confirmed)
        {
            await pictureService.DeleteAsync(id);
            await LoadDataAsync();
        }
    }

    private string GetPictureTypeText(PictureType type)
    {
        return type.ToString();
    }
}
