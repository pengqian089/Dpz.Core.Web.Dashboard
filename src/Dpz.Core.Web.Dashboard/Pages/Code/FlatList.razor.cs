using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Code;

public partial class FlatList(
    ICodeService codeService,
    IAppDialogService dialogService,
    NavigationManager navigationManager
) : IDisposable
{
    [Parameter]
    public EventCallback<List<string>> OnViewFile { get; set; }

    [Parameter]
    public EventCallback OnQueryChanged { get; set; }

    private CodeFlatRequest _request = new CodeFlatRequest
    {
        PageIndex = 1,
        PageSize = 20,
        IsDescending = true,
    };

    private IPagedList<CodeFileSystemEntryListResponse>? _pagedList;
    private bool _isLoading;
    private List<string[]> _availablePaths = [];
    private string _selectedPath = "";
    private bool _isNavigating;
    private string _sortFieldValue = "";

    protected override async Task OnInitializedAsync()
    {
        navigationManager.LocationChanged += OnLocationChanged;
        await LoadPathsAsync();
        await LoadFromUrlAsync();
    }

    public void Dispose()
    {
        navigationManager.LocationChanged -= OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        if (_isNavigating)
        {
            return;
        }

        _ = InvokeAsync(async () =>
        {
            await LoadFromUrlAsync();
            StateHasChanged();
        });
    }

    private async Task LoadFromUrlAsync()
    {
        var uri = new Uri(navigationManager.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        _request.PageIndex = int.TryParse(query["page"], out var page) ? page : 1;
        _request.Name = query["name"];
        _request.Extension = query["ext"];
        _request.IsDescending = query["desc"] != "false";

        if (Enum.TryParse<CodeFileSortField>(query["sort"], out var sortField))
        {
            _request.SortField = sortField;
        }

        var pathParam = query["path"];
        if (!string.IsNullOrWhiteSpace(pathParam))
        {
            _request.PathSegments = pathParam.Split('/', StringSplitOptions.RemoveEmptyEntries);
            _selectedPath = pathParam;
        }
        else
        {
            _request.PathSegments = null;
            _selectedPath = "";
        }

        UpdateSortFieldValue();
        await LoadDataAsync(updateUrl: false);
    }

    private async Task LoadPathsAsync()
    {
        try
        {
            _availablePaths = await codeService.GetDirectoriesAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载路径失败：{ex.Message}", ToastType.Error);
        }
    }

    private async Task LoadDataAsync(bool updateUrl = true)
    {
        _isLoading = true;
        try
        {
            _pagedList = await codeService.GetFlatListAsync(_request);

            if (updateUrl)
            {
                UpdateUrl();
            }

            if (OnQueryChanged.HasDelegate)
            {
                await OnQueryChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void UpdateUrl()
    {
        try
        {
            _isNavigating = true;

            var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
            var basePath = uri.GetLeftPart(UriPartial.Path);
            var queryParams = new List<string> { "view=list" };

            if (_request.PageIndex > 1)
            {
                queryParams.Add($"page={_request.PageIndex}");
            }

            if (!string.IsNullOrWhiteSpace(_request.Name))
            {
                queryParams.Add($"name={HttpUtility.UrlEncode(_request.Name)}");
            }

            if (!string.IsNullOrWhiteSpace(_request.Extension))
            {
                queryParams.Add($"ext={HttpUtility.UrlEncode(_request.Extension)}");
            }

            if (_request.SortField.HasValue)
            {
                queryParams.Add($"sort={_request.SortField}");
            }

            if (!_request.IsDescending)
            {
                queryParams.Add("desc=false");
            }

            if (_request.PathSegments is { Length: > 0 })
            {
                queryParams.Add($"path={HttpUtility.UrlEncode(string.Join("/", _request.PathSegments))}");
            }

            var queryString = "?" + string.Join("&", queryParams);
            navigationManager.NavigateTo(basePath + queryString, replace: false);
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async Task SearchAsync()
    {
        _request.PageIndex = 1;
        await LoadDataAsync();
    }

    private async Task ResetFiltersAsync()
    {
        _request = new CodeFlatRequest
        {
            PageIndex = 1,
            PageSize = 20,
            IsDescending = true,
        };
        _selectedPath = "";
        await LoadDataAsync();
    }

    private async Task HandlePathChanged()
    {
        if (string.IsNullOrWhiteSpace(_selectedPath))
        {
            _request.PathSegments = null;
        }
        else
        {
            _request.PathSegments = _selectedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        }
        _request.PageIndex = 1;
        await LoadDataAsync();
    }

    private async Task FilterByPathAsync(List<string> pathSegments)
    {
        _request.PathSegments = pathSegments.ToArray();
        _selectedPath = string.Join("/", pathSegments);
        _request.PageIndex = 1;
        await LoadDataAsync();
    }

    private async Task ToggleSortAsync(CodeFileSortField? field)
    {
        if (field == null)
        {
            return;
        }

        if (_request.SortField == field)
        {
            _request.IsDescending = !_request.IsDescending;
        }
        else
        {
            _request.SortField = field;
            _request.IsDescending = true;
        }

        _request.PageIndex = 1;
        await LoadDataAsync();
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            await SearchAsync();
        }
    }

    private async Task ViewFileAsync(CodeFileSystemEntryListResponse file)
    {
        if (OnViewFile.HasDelegate)
        {
            await OnViewFile.InvokeAsync(file.PathSegments);
        }
    }

    private async Task EditNoteAsync(CodeFileSystemEntryListResponse item)
    {
        var model = new CodeSaveModel
        {
            Name = item.Name,
            Path = item.ParentPathSegments.ToArray(),
            Note = item.Description,
        };

        var result = await dialogService.ShowComponentAsync<bool>(
            "编辑说明",
            BuildNoteForm(model),
            "600px"
        );

        if (result)
        {
            await LoadDataAsync(updateUrl: false);
        }
    }

    private async Task HandlePageChangedAsync(int page)
    {
        _request.PageIndex = page;
        await LoadDataAsync();
    }

    private async Task HandleSortChanged()
    {
        (_request.SortField, _request.IsDescending) = _sortFieldValue switch
        {
            "size-desc" => ((CodeFileSortField?)CodeFileSortField.Size, true),
            "size-asc" => ((CodeFileSortField?)CodeFileSortField.Size, false),
            "created-desc" => ((CodeFileSortField?)CodeFileSortField.CreatedTime, true),
            "created-asc" => ((CodeFileSortField?)CodeFileSortField.CreatedTime, false),
            "write-desc" => ((CodeFileSortField?)CodeFileSortField.LastWriteTime, true),
            "write-asc" => ((CodeFileSortField?)CodeFileSortField.LastWriteTime, false),
            "update-desc" => ((CodeFileSortField?)CodeFileSortField.LastUpdateTime, true),
            "update-asc" => ((CodeFileSortField?)CodeFileSortField.LastUpdateTime, false),
            "ai-desc" => ((CodeFileSortField?)CodeFileSortField.AiAnalyzeTime, true),
            "ai-asc" => ((CodeFileSortField?)CodeFileSortField.AiAnalyzeTime, false),
            _ => ((CodeFileSortField?)null, true)
        };
        _request.PageIndex = 1;
        await LoadDataAsync();
    }

    private void UpdateSortFieldValue()
    {
        _sortFieldValue = (_request.SortField, _request.IsDescending) switch
        {
            (CodeFileSortField.Size, true) => "size-desc",
            (CodeFileSortField.Size, false) => "size-asc",
            (CodeFileSortField.CreatedTime, true) => "created-desc",
            (CodeFileSortField.CreatedTime, false) => "created-asc",
            (CodeFileSortField.LastWriteTime, true) => "write-desc",
            (CodeFileSortField.LastWriteTime, false) => "write-asc",
            (CodeFileSortField.LastUpdateTime, true) => "update-desc",
            (CodeFileSortField.LastUpdateTime, false) => "update-asc",
            (CodeFileSortField.AiAnalyzeTime, true) => "ai-desc",
            (CodeFileSortField.AiAnalyzeTime, false) => "ai-asc",
            _ => ""
        };
    }

    private static RenderFragment BuildNoteForm(CodeSaveModel model)
    {
        return builder =>
        {
            builder.OpenComponent<NoteForm>(0);
            builder.AddAttribute(1, "Model", model);
            builder.CloseComponent();
        };
    }

    private static string GetDisplayPath(List<string> pathSegments)
    {
        if (pathSegments == null || pathSegments.Count == 0)
        {
            return "/";
        }
        return "/" + string.Join("/", pathSegments);
    }

    private static string FormatFileSize(long size)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = size;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private static string GetNoteText(string? note)
    {
        return string.IsNullOrWhiteSpace(note) ? "-" : note;
    }

    private static string GetFileIcon(string? extension)
    {
        return extension?.ToLower() switch
        {
            ".cs" => "fa-file-code",
            ".js" or ".ts" or ".jsx" or ".tsx" => "fa-brands fa-js",
            ".css" or ".scss" or ".sass" or ".less" => "fa-brands fa-css3-alt",
            ".html" or ".htm" => "fa-brands fa-html5",
            ".json" => "fa-file-code",
            ".md" or ".markdown" => "fa-file-alt",
            ".xml" => "fa-file-code",
            ".sql" => "fa-database",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" or ".webp" => "fa-file-image",
            ".pdf" => "fa-file-pdf",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "fa-file-archive",
            _ => "fa-file-code"
        };
    }
}
