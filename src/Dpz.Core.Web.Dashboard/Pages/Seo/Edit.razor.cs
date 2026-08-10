using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Models.Seo;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Seo;

public partial class Edit(IPageMetadataService seoService, IAppDialogService dialogService)
{
    [Parameter]
    public PageMetadataResponse? Model { get; set; }

    [Parameter]
    public bool ForcePublicMetadata { get; set; }

    [Parameter]
    public IReadOnlyList<PageRouteDefinitionResponse> Routes { get; set; } = [];

    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    private string? _id;
    private bool _isPublicMetadata;
    private bool _applyToUnconfiguredPages;
    private PageMetadataInheritanceMode _inheritanceMode = PageMetadataInheritanceMode.Inherit;
    private string _title = "";
    private List<string> _keywords = [];
    private string _description = "";
    private List<MetaItem> _metas = [];
    private string _routeSearch = "";
    private string _selectedRouteKey = "";
    private PageRouteDefinitionResponse? _selectedRoute;
    private Dictionary<string, string> _parameterValues = new(StringComparer.OrdinalIgnoreCase);
    private PageMetadataResponse? _preview;
    private string? _previewError;
    private bool _isPreviewing;
    private bool _isSubmitting;

    private bool IsPublicMetadata => _isPublicMetadata;

    private string FormTitle
    {
        get
        {
            if (IsPublicMetadata)
            {
                return Model == null ? "配置公共 SEO" : "编辑公共 SEO";
            }

            return Model == null ? "新增页面 SEO" : "编辑页面 SEO";
        }
    }

    private string FormSubtitle =>
        IsPublicMetadata ? "作为所有页面可继承的 SEO 基底" : "从路由目录选择页面并配置匹配参数";

    private IEnumerable<PageRouteDefinitionResponse> FilteredRoutes
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_routeSearch))
            {
                return Routes;
            }

            return Routes.Where(x =>
                Contains(x.Area, _routeSearch)
                || Contains(x.Controller, _routeSearch)
                || Contains(x.Action, _routeSearch)
                || Contains(x.RouteKey, _routeSearch)
                || x.Endpoints.Any(endpoint => Contains(endpoint.Template, _routeSearch))
            );
        }
    }

    private string PreviewUrl
    {
        get
        {
            var baseUrl = Program.WebHost.TrimEnd('/');
            if (IsPublicMetadata)
            {
                return $"{baseUrl}/...";
            }

            if (_selectedRoute == null)
            {
                return $"{baseUrl}/controller/action";
            }

            var template = _selectedRoute.Endpoints.FirstOrDefault()?.Template;
            if (string.IsNullOrWhiteSpace(template))
            {
                return $"{baseUrl}/{_selectedRoute.Controller}/{_selectedRoute.Action}";
            }

            var path = template.TrimStart('/');
            foreach (var parameter in _selectedRoute.Parameters)
            {
                var value = GetParameterValue(parameter.Name);
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = parameter.Name;
                }

                path = path.Replace("{" + parameter.Name + "}", Uri.EscapeDataString(value));
                path = path.Replace("{" + parameter.Name + "?}", Uri.EscapeDataString(value));
            }

            var query = _selectedRoute
                .Parameters.Where(x =>
                    x.Source.HasFlag(PageRouteParameterSource.Query)
                    && !x.Source.HasFlag(PageRouteParameterSource.Route)
                )
                .Select(x => new { x.Name, Value = GetParameterValue(x.Name) })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(x.Value)}")
                .ToList();

            return query.Count == 0
                ? $"{baseUrl}/{path}"
                : $"{baseUrl}/{path}?{string.Join("&", query)}";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        InitializeForm();
        await RefreshPreviewAsync();
        await base.OnInitializedAsync();
    }

    private void InitializeForm()
    {
        _isPublicMetadata = ForcePublicMetadata || Model?.IsPublicMetadata == true;

        if (Model == null)
        {
            SelectDefaultRoute();
            return;
        }

        _id = Model.Id;
        _applyToUnconfiguredPages = Model.ApplyToUnconfiguredPages;
        _inheritanceMode = Model.InheritanceMode;
        _title = Model.Title ?? "";
        _keywords = new List<string>(Model.Keywords);
        _description = Model.Description ?? "";
        _metas = Model.Metas.Select(m => new MetaItem { Key = m.Key, Value = m.Value }).ToList();

        if (IsPublicMetadata)
        {
            _inheritanceMode = PageMetadataInheritanceMode.Inherit;
            return;
        }

        _selectedRouteKey = FindRouteKey(Model.Route) ?? FindRouteKey(Model.Relations) ?? "";
        ApplySelectedRoute(true);

        if (Model.Route?.Parameters.Count > 0)
        {
            foreach (var (key, value) in Model.Route.Parameters)
            {
                _parameterValues[key] = value;
            }
        }
    }

    private void SelectDefaultRoute()
    {
        if (IsPublicMetadata || Routes.Count == 0)
        {
            return;
        }

        _selectedRouteKey = Routes[0].RouteKey;
        ApplySelectedRoute(false);
    }

    private string? FindRouteKey(PageMetadataRoute? route)
    {
        if (route == null)
        {
            return null;
        }

        return Routes.FirstOrDefault(x => RouteMatches(x, route))?.RouteKey;
    }

    private string? FindRouteKey(IReadOnlyList<string> relations)
    {
        if (relations.Count < 2)
        {
            return null;
        }

        var controller = relations[0];
        var action = relations[1];
        return Routes
            .FirstOrDefault(x =>
                string.Equals(x.Controller, controller, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Action, action, StringComparison.OrdinalIgnoreCase)
            )
            ?.RouteKey;
    }

    private static bool RouteMatches(
        PageRouteDefinitionResponse route,
        PageMetadataRoute metadataRoute
    )
    {
        return string.Equals(route.Area, metadataRoute.Area, StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                route.Controller,
                metadataRoute.Controller,
                StringComparison.OrdinalIgnoreCase
            )
            && string.Equals(
                route.Action,
                metadataRoute.Action,
                StringComparison.OrdinalIgnoreCase
            );
    }

    private async Task HandleRouteChanged(ChangeEventArgs e)
    {
        _selectedRouteKey = e.Value?.ToString() ?? "";
        ApplySelectedRoute(false);
        await RefreshPreviewAsync();
    }

    private void ApplySelectedRoute(bool preserveValues)
    {
        var existingValues = preserveValues
            ? new Dictionary<string, string>(_parameterValues, StringComparer.OrdinalIgnoreCase)
            : [];

        _selectedRoute = Routes.FirstOrDefault(x =>
            string.Equals(x.RouteKey, _selectedRouteKey, StringComparison.Ordinal)
        );
        _parameterValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (_selectedRoute == null)
        {
            return;
        }

        foreach (var parameter in _selectedRoute.Parameters)
        {
            _parameterValues[parameter.Name] = existingValues.TryGetValue(
                parameter.Name,
                out var value
            )
                ? value
                : "";
        }
    }

    private void SetParameterValue(string name, string value)
    {
        _parameterValues[name] = value;
    }

    private string GetParameterValue(string name)
    {
        return _parameterValues.TryGetValue(name, out var value) ? value : "";
    }

    private void SetInheritanceMode(PageMetadataInheritanceMode mode)
    {
        _inheritanceMode = mode;
    }

    private void AddMeta()
    {
        _metas.Add(new MetaItem());
    }

    private void RemoveMeta(MetaItem meta)
    {
        _metas.Remove(meta);
    }

    private async Task RefreshPreviewAsync()
    {
        _previewError = null;
        _preview = null;

        if (IsPublicMetadata)
        {
            _preview = BuildMetadataModel();
            return;
        }

        var route = BuildRoute();
        if (route == null)
        {
            _previewError = "请选择一个已扫描的页面路由";
            return;
        }

        _isPreviewing = true;
        StateHasChanged();

        try
        {
            _preview = await seoService.PreviewAsync(
                new SeoPreviewRequest { Route = route, ExplicitMetadata = BuildMetadataModel() }
            );
        }
        catch (Exception ex)
        {
            _previewError = ex.Message;
        }
        finally
        {
            _isPreviewing = false;
            StateHasChanged();
        }
    }

    private async Task SaveAsync()
    {
        var route = BuildRoute();
        if (!IsPublicMetadata && route == null)
        {
            dialogService.Toast("请选择一个已扫描的页面路由", ToastType.Warning);
            return;
        }

        _isSubmitting = true;
        StateHasChanged();

        try
        {
            var metadata = BuildMetadataModel();
            var request = new SeoSaveRequest
            {
                Id = metadata.Id,
                IsPublicMetadata = metadata.IsPublicMetadata,
                ApplyToUnconfiguredPages = metadata.ApplyToUnconfiguredPages,
                InheritanceMode = metadata.InheritanceMode,
                Route = metadata.Route,
                Title = metadata.Title,
                Keywords = metadata.Keywords,
                Description = metadata.Description,
                Metas = metadata.Metas,
                Relations = [],
            };

            await seoService.SaveAsync(request);
            dialogService.Toast("保存成功", ToastType.Success);
            CloseDialog?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private PageMetadataResponse BuildMetadataModel()
    {
        return new PageMetadataResponse
        {
            Id = _id,
            IsPublicMetadata = IsPublicMetadata,
            ApplyToUnconfiguredPages = IsPublicMetadata && _applyToUnconfiguredPages,
            InheritanceMode = IsPublicMetadata
                ? PageMetadataInheritanceMode.Inherit
                : _inheritanceMode,
            Route = IsPublicMetadata ? null : BuildRoute(),
            Title = string.IsNullOrWhiteSpace(_title) ? null : _title.Trim(),
            Keywords = _keywords
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Description = string.IsNullOrWhiteSpace(_description) ? null : _description.Trim(),
            Metas = BuildMetas(),
            Relations = [],
        };
    }

    private PageMetadataRoute? BuildRoute()
    {
        if (IsPublicMetadata || _selectedRoute == null)
        {
            return null;
        }

        var parameters = _selectedRoute
            .Parameters.Select(x => new { x.Name, Value = GetParameterValue(x.Name).Trim() })
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

        return new PageMetadataRoute
        {
            Area = _selectedRoute.Area,
            Controller = _selectedRoute.Controller,
            Action = _selectedRoute.Action,
            Parameters = parameters,
        };
    }

    private Dictionary<string, string> BuildMetas()
    {
        return _metas
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key.Trim(), StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Last().Value.Trim(), StringComparer.Ordinal);
    }

    private void Cancel()
    {
        CloseDialog?.Invoke(false);
    }

    private static bool Contains(string? value, string searchText)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
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

    private sealed class MetaItem
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
