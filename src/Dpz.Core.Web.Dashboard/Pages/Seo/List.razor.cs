using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Seo;

public partial class List(
    IPageMetadataService seoService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private const int PageSize = 15;
    private string _searchText = "";
    private List<PageMetadataResponse> _items = [];
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

        _searchText = query["search"] ?? "";
    }

    private void UpdateUrl()
    {
        var baseUri = navigation.ToAbsoluteUri("/seo").GetLeftPart(UriPartial.Path);
        var queryParams = new List<string>();

        if (_pageIndex > 1)
        {
            queryParams.Add($"page={_pageIndex}");
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            queryParams.Add($"search={Uri.EscapeDataString(_searchText)}");
        }

        var url = queryParams.Count > 0 ? $"{baseUri}?{string.Join("&", queryParams)}" : baseUri;

        navigation.NavigateTo(url, false);
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var result = await seoService.GetPageAsync(_searchText, _pageIndex, PageSize);
            _items = result.ToList();
            _totalCount = result.TotalItemCount;
            _totalPages = result.TotalPageCount;
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

    private async Task SearchAsync()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task ReloadAsync()
    {
        await LoadDataAsync();
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SearchAsync();
        }
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task RefreshCacheAsync()
    {
        try
        {
            await seoService.RefreshCacheAsync();
            dialogService.Toast("缓存刷新成功", ToastType.Success);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"刷新失败：{ex.Message}", ToastType.Error);
        }
    }

    private async Task AddAsync()
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "新增 SEO 元数据",
            BuildEditForm(null),
            "900px"
        );

        if (result)
        {
            await LoadDataAsync();
        }
    }

    private async Task EditAsync(PageMetadataResponse item)
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "编辑 SEO 元数据",
            BuildEditForm(item),
            "900px"
        );

        if (result)
        {
            await LoadDataAsync();
        }
    }

    private async Task DeleteAsync(string id)
    {
        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (confirmed)
        {
            try
            {
                await seoService.DeleteAsync(id);
                dialogService.Toast("删除成功", ToastType.Success);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                dialogService.Toast($"删除失败：{ex.Message}", ToastType.Error);
            }
        }
    }

    private async Task ViewMetasAsync(PageMetadataResponse item)
    {
        var content = BuildMetasPreview(item);
        await dialogService.ShowComponentAsync("元数据详情", content, "720px");
    }

    private static RenderFragment BuildEditForm(PageMetadataResponse? model)
    {
        return builder =>
        {
            builder.OpenComponent<Edit>(0);
            builder.AddAttribute(1, "Model", model);
            builder.CloseComponent();
        };
    }

    private static RenderFragment BuildMetasPreview(PageMetadataResponse item)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "seo-metas-preview");

            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "seo-metas-preview__section");
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "seo-metas-preview__label");
            builder.AddContent(6, "关联路径");
            builder.CloseElement();
            builder.OpenElement(7, "div");
            builder.AddAttribute(8, "class", "seo-metas-preview__value");
            builder.AddContent(9, item.RelationPath ?? "-");
            builder.CloseElement();
            builder.CloseElement();

            if (item.Metas.Count > 0)
            {
                builder.OpenElement(10, "div");
                builder.AddAttribute(11, "class", "seo-metas-preview__section");
                builder.OpenElement(12, "div");
                builder.AddAttribute(13, "class", "seo-metas-preview__label");
                builder.AddContent(14, "其他元数据");
                builder.CloseElement();
                builder.OpenElement(15, "div");
                builder.AddAttribute(16, "class", "seo-metas-preview__metas");

                foreach (var meta in item.Metas)
                {
                    builder.OpenElement(17, "div");
                    builder.AddAttribute(18, "class", "seo-metas-preview__meta-item");
                    builder.OpenElement(19, "span");
                    builder.AddAttribute(20, "class", "seo-metas-preview__meta-key");
                    builder.AddContent(21, meta.Key);
                    builder.CloseElement();
                    builder.OpenElement(22, "span");
                    builder.AddAttribute(23, "class", "seo-metas-preview__meta-value");
                    builder.AddContent(24, meta.Value);
                    builder.CloseElement();
                    builder.CloseElement();
                }

                builder.CloseElement();
                builder.CloseElement();
            }

            builder.CloseElement();
        };
    }
}
