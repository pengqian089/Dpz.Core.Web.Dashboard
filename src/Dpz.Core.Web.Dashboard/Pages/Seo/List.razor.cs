using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Models.Seo;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Seo;

public partial class List(
    IPageMetadataService seoService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    private const int PageSize = 15;
    private const int PublicMetadataLookupSize = 200;

    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private string _searchText = "";
    private List<PageMetadataResponse> _items = [];
    private IReadOnlyList<PageRouteDefinitionResponse> _routes = [];
    private PageMetadataResponse? _publicMetadata;
    private bool _isLoading = true;

    private int RouteParameterCount => _routes.Sum(x => x.Parameters.Count);

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
            _routes = await seoService.GetRoutesAsync();
            await LoadPublicMetadataAsync();

            var result = await seoService.GetPageAsync(_searchText, _pageIndex, PageSize);
            _items = result.Where(x => !x.IsPublicMetadata).ToList();
            _totalCount = CalculatePageMetadataCount(result.TotalItemCount);
            _totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
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

    private async Task LoadPublicMetadataAsync()
    {
        var result = await seoService.GetPageAsync(null, 1, PublicMetadataLookupSize);
        _publicMetadata = result.FirstOrDefault(x => x.IsPublicMetadata);
    }

    private int CalculatePageMetadataCount(int totalCount)
    {
        if (_publicMetadata == null || !string.IsNullOrWhiteSpace(_searchText))
        {
            return totalCount;
        }

        return Math.Max(0, totalCount - 1);
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
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"刷新失败：{ex.Message}", ToastType.Error);
        }
    }

    private Task AddAsync()
    {
        return ShowEditorAsync(null, false);
    }

    private Task EditPublicAsync()
    {
        return ShowEditorAsync(_publicMetadata, true);
    }

    private Task EditAsync(PageMetadataResponse item)
    {
        return ShowEditorAsync(item, item.IsPublicMetadata);
    }

    private async Task ShowEditorAsync(PageMetadataResponse? model, bool forcePublicMetadata)
    {
        var title =
            forcePublicMetadata ? "公共 SEO 元数据"
            : model == null ? "新增页面 SEO"
            : "编辑页面 SEO";

        var result = await dialogService.ShowComponentAsync<bool>(
            title,
            BuildEditForm(model, forcePublicMetadata, _routes),
            "1120px"
        );

        if (result)
        {
            await LoadDataAsync();
        }
    }

    private async Task DeleteAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            dialogService.Toast("缺少元数据 ID，无法删除", ToastType.Warning);
            return;
        }

        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (!confirmed)
        {
            return;
        }

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

    private async Task ViewMetasAsync(PageMetadataResponse item)
    {
        var content = BuildMetasPreview(item);
        await dialogService.ShowComponentAsync("元数据详情", content, "760px");
    }

    private static RenderFragment BuildEditForm(
        PageMetadataResponse? model,
        bool forcePublicMetadata,
        IReadOnlyList<PageRouteDefinitionResponse> routes
    )
    {
        return builder =>
        {
            builder.OpenComponent<Edit>(0);
            builder.AddAttribute(1, "Model", model);
            builder.AddAttribute(2, "ForcePublicMetadata", forcePublicMetadata);
            builder.AddAttribute(3, "Routes", routes);
            builder.CloseComponent();
        };
    }

    private static string FormatRoute(PageMetadataResponse item)
    {
        if (item.IsPublicMetadata)
        {
            return "公共元数据";
        }

        if (item.Route != null)
        {
            return FormatRoute(item.Route);
        }

        return item.RouteKey ?? item.RelationPath ?? "-";
    }

    private static string FormatRoute(PageMetadataRoute route)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(route.Area))
        {
            parts.Add(route.Area);
        }

        parts.Add(route.Controller ?? "-");
        parts.Add(route.Action ?? "-");
        return string.Join("/", parts);
    }

    private static string FormatRoute(PageRouteDefinitionResponse route)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(route.Area))
        {
            parts.Add(route.Area);
        }

        parts.Add(route.Controller);
        parts.Add(route.Action);
        return string.Join("/", parts);
    }

    private static string FormatParameters(PageMetadataResponse item)
    {
        if (item.Route?.Parameters.Count > 0)
        {
            return string.Join(", ", item.Route.Parameters.Select(x => $"{x.Key}={x.Value}"));
        }

        if (item.Relations.Count > 0 && item.Route == null)
        {
            return string.Join(" / ", item.Relations);
        }

        return "未限定参数";
    }

    private static string FormatParameterSource(PageRouteParameterSource source)
    {
        if (source == PageRouteParameterSource.None)
        {
            return "None";
        }

        var sources = new List<string>();
        if (source.HasFlag(PageRouteParameterSource.Route))
        {
            sources.Add("Route");
        }

        if (source.HasFlag(PageRouteParameterSource.Query))
        {
            sources.Add("Query");
        }

        return string.Join(" + ", sources);
    }

    private static string GetInheritanceLabel(PageMetadataResponse item)
    {
        if (item.IsPublicMetadata)
        {
            return item.ApplyToUnconfiguredPages ? "公共默认" : "公共基底";
        }

        return item.InheritanceMode == PageMetadataInheritanceMode.Ignore ? "不继承" : "继承公共";
    }

    private static string GetInheritanceClass(PageMetadataResponse item)
    {
        if (item.IsPublicMetadata)
        {
            return item.ApplyToUnconfiguredPages ? "seo-status--success" : "seo-status--info";
        }

        return item.InheritanceMode == PageMetadataInheritanceMode.Ignore
            ? "seo-status--warning"
            : "seo-status--success";
    }

    private static string Clip(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }

    private static RenderFragment BuildMetasPreview(PageMetadataResponse item)
    {
        return builder =>
        {
            var sequence = 0;
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "seo-metas-preview");

            AddPreviewSection(builder, ref sequence, "路由", FormatRoute(item));
            AddPreviewSection(builder, ref sequence, "参数", FormatParameters(item));
            AddPreviewSection(builder, ref sequence, "继承", GetInheritanceLabel(item));
            AddPreviewSection(builder, ref sequence, "标题", item.Title ?? "-");
            AddPreviewSection(builder, ref sequence, "描述", item.Description ?? "-");

            if (item.Keywords.Count > 0)
            {
                AddPreviewSection(
                    builder,
                    ref sequence,
                    "关键词",
                    string.Join(", ", item.Keywords)
                );
            }

            if (item.Metas.Count > 0)
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "seo-metas-preview__section");
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "seo-metas-preview__label");
                builder.AddContent(sequence++, "其他元数据");
                builder.CloseElement();
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "seo-metas-preview__metas");

                foreach (var meta in item.Metas)
                {
                    builder.OpenElement(sequence++, "div");
                    builder.AddAttribute(sequence++, "class", "seo-metas-preview__meta-item");
                    builder.OpenElement(sequence++, "span");
                    builder.AddAttribute(sequence++, "class", "seo-metas-preview__meta-key");
                    builder.AddContent(sequence++, meta.Key);
                    builder.CloseElement();
                    builder.OpenElement(sequence++, "span");
                    builder.AddAttribute(sequence++, "class", "seo-metas-preview__meta-value");
                    builder.AddContent(sequence++, meta.Value);
                    builder.CloseElement();
                    builder.CloseElement();
                }

                builder.CloseElement();
                builder.CloseElement();
            }

            builder.CloseElement();
        };
    }

    private static void AddPreviewSection(
        RenderTreeBuilder builder,
        ref int sequence,
        string label,
        string value
    )
    {
        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", "seo-metas-preview__section");
        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", "seo-metas-preview__label");
        builder.AddContent(sequence++, label);
        builder.CloseElement();
        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", "seo-metas-preview__value");
        builder.AddContent(sequence++, value);
        builder.CloseElement();
        builder.CloseElement();
    }
}
