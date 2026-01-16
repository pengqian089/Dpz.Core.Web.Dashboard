using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.DynamicPage;

public partial class List(
    IDynamicPageService dynamicPageService,
    IAppDialogService dialogService
)
{
    private const int PageSize = 12;
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private bool _isLoading = true;
    private string _keyword = "";
    private string _tempKeyword = "";
    private List<DynamicPageListModel> _pages = [];

    protected override async Task OnInitializedAsync()
    {
        _tempKeyword = _keyword;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            if (_keyword != _tempKeyword)
            {
                _tempKeyword = _keyword;
                _pageIndex = 1;
            }

            var list = await dynamicPageService.GetPageAsync(_keyword, _pageIndex, PageSize);
            _pages = list.ToList();
            _totalCount = list.TotalItemCount;
            _totalPages = list.TotalPageCount;
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task Search()
    {
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

    private async Task HandlePageChanged(int page)
    {
        if (page < 1 || page > _totalPages || page == _pageIndex)
        {
            return;
        }

        _pageIndex = page;
        await LoadDataAsync();
    }

    private async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            dialogService.Toast("参数错误", ToastType.Warning);
            return;
        }

        var confirmed = await dialogService.ConfirmAsync(
            "删除后不能恢复，确定删除？",
            "确认删除"
        );
        if (!confirmed)
        {
            return;
        }

        try
        {
            await dynamicPageService.DeleteAsync(id);
            dialogService.Toast("删除成功", ToastType.Success);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"删除失败：{ex.Message}", ToastType.Error);
        }
    }

    private static string GetCreatorName(DynamicPageListModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Creator?.Name))
        {
            return model.Creator.Name;
        }

        return "未知";
    }

    private static string GetContentType(DynamicPageListModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.ContentTypeStr))
        {
            return model.ContentTypeStr;
        }

        return "未知";
    }

    private static string BuildPreviewUrl(string id)
    {
        var baseUrl = Program.WebHost.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(id))
        {
            return $"{baseUrl}/act";
        }

        return $"{baseUrl}/act/{id}";
    }
}