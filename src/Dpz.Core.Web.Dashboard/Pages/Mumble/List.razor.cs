using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Mumble;

public partial class List(
    IMumbleService mumbleService,
    NavigationManager navigation,
    IAppDialogService dialogService
) : ComponentBase
{
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;

    private const int PageSize = 10;

    private string _content = "";

    private IPagedList<MumbleModel> _data = PagedList<MumbleModel>.Empty();

    private bool _isLoading = true;

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

        _content = query["content"] ?? "";
    }

    private void UpdateUrl()
    {
        var baseUri = navigation.ToAbsoluteUri("/mumble").GetLeftPart(UriPartial.Path);
        var queryParams = new List<string>();

        if (_pageIndex > 1)
        {
            queryParams.Add($"page={_pageIndex}");
        }

        if (!string.IsNullOrWhiteSpace(_content))
        {
            queryParams.Add($"content={Uri.EscapeDataString(_content)}");
        }

        var url = queryParams.Count > 0 ? $"{baseUri}?{string.Join("&", queryParams)}" : baseUri;

        navigation.NavigateTo(url, false);
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            _data = await mumbleService.GetPageAsync(_content, _pageIndex, PageSize);
            _totalCount = _data.TotalItemCount;
            _totalPages = _data.TotalPageCount;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnSearch()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await OnSearch();
        }
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task DeleteAsync(string id)
    {
        var result = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (result)
        {
            await mumbleService.DeleteAsync(id);
            await LoadDataAsync();
            dialogService.Toast("删除成功", ToastType.Success);
        }
    }

    private Task Preview(MumbleModel model)
    {
        return dialogService.ShowComponentAsync(
            "预览",
            builder =>
            {
                builder.OpenComponent<MarkdownPreview>(0);
                builder.AddAttribute(1, "Markdown", model.Markdown);
                builder.CloseComponent();
            },
            "800px"
        );
    }
}
