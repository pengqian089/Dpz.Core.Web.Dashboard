using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Code;

public partial class Code(
    ICodeService codeService,
    IAppDialogService dialogService,
    NavigationManager navigationManager
) : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = navigationManager;
    private CodeNoteTree? _treeData;
    private bool _isLoading;
    private List<string> _currentPath = [];
    private string _searchWord = "";
    private string _tempSearch = "";
    private bool _isNavigating = false;

    private bool IsUnavailable =>
        _treeData is null
        || _treeData.Type == FileSystemType.NoExists
        || _treeData.Type == FileSystemType.NoSupport;

    private string UnavailableMessage =>
        _treeData?.Type == FileSystemType.NoSupport
            ? "当前内容暂不支持展示"
            : "内容不存在或已被移除";

    private int TotalItems =>
        _treeData == null ? 0 : _treeData.Directories.Count + _treeData.Files.Count;

    private string EditorId =>
        _currentPath.Count == 0 ? "code-root" : string.Join('-', _currentPath);

    protected override async Task OnInitializedAsync()
    {
        // 注册导航事件监听
        Navigation.LocationChanged += OnLocationChanged;

        // 从URL参数读取路径
        await LoadFromUrlAsync();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        // 避免在UpdateUrl时触发循环
        if (_isNavigating)
        {
            return;
        }

        // 异步加载新路径
        _ = InvokeAsync(async () =>
        {
            await LoadFromUrlAsync();
            StateHasChanged();
        });
    }

    private async Task LoadFromUrlAsync()
    {
        var uri = new Uri(Navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var pathParam = query["path"];

        string[]? initialPath = null;
        if (!string.IsNullOrWhiteSpace(pathParam))
        {
            initialPath = pathParam.Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        await LoadTreeDataAsync(initialPath, updateUrl: false);
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }

    private async Task LoadTreeDataAsync(IEnumerable<string>? path, bool updateUrl = true)
    {
        _isLoading = true;
        StateHasChanged();
        var currentPath = path?.ToArray() ?? [];
        try
        {
            _treeData = await codeService.GetTreeAsync(currentPath);
            _currentPath = currentPath.ToList();
            _searchWord = "";
            _tempSearch = "";

            // 同步URL参数
            if (updateUrl)
            {
                UpdateUrl(currentPath);
            }
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

    private void UpdateUrl(string[] path)
    {
        try
        {
            _isNavigating = true;

            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            var basePath = uri.GetLeftPart(UriPartial.Path);

            if (path.Length > 0)
            {
                var pathParam = string.Join("/", path);
                var newUrl = $"{basePath}?path={HttpUtility.UrlEncode(pathParam)}";
                Navigation.NavigateTo(newUrl, replace: false);
            }
            else
            {
                Navigation.NavigateTo(basePath, replace: false);
            }
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async Task Reload()
    {
        if (!string.IsNullOrWhiteSpace(_tempSearch))
        {
            _searchWord = _tempSearch;
            await SearchInternalAsync(force: true);
            return;
        }

        await LoadTreeDataAsync(_currentPath);
    }

    private async Task BackToRoot()
    {
        await LoadTreeDataAsync(null);
    }

    private async Task BackToParent()
    {
        if (_currentPath.Count == 0)
        {
            await LoadTreeDataAsync(null);
            return;
        }

        var parentPath = _currentPath.Take(_currentPath.Count - 1).ToArray();
        await LoadTreeDataAsync(parentPath);
    }

    private Task Search()
    {
        return SearchInternalAsync(force: false);
    }

    private async Task SearchInternalAsync(bool force)
    {
        if (string.IsNullOrWhiteSpace(_searchWord))
        {
            if (string.IsNullOrWhiteSpace(_tempSearch))
            {
                dialogService.Toast("请输入关键字", ToastType.Warning);
                return;
            }

            _tempSearch = "";
            await LoadTreeDataAsync(null);
            return;
        }

        if (!force && string.Equals(_searchWord, _tempSearch, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _isLoading = true;
        StateHasChanged();
        try
        {
            _treeData = await codeService.SearchAsync(_searchWord);
            _tempSearch = _searchWord;
            _currentPath = _treeData?.CurrentPaths ?? [];
        }
        catch (Exception ex)
        {
            dialogService.Toast($"搜索失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            await Search();
        }
    }

    private async Task SaveNoteAsync(IEnumerable<string>? path, string name, string? note)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            dialogService.Toast("参数错误", ToastType.Warning);
            return;
        }

        var model = new CodeSaveModel
        {
            Name = name,
            Note = note,
            Path = path?.ToArray(),
        };

        var result = await dialogService.ShowComponentAsync<bool>(
            "保存说明",
            BuildNoteForm(model),
            "560px"
        );

        if (result)
        {
            await Reload();
        }
    }

    private async Task EditCurrentNote()
    {
        var model = BuildCurrentNoteModel();
        await SaveNoteAsync(model.Path, model.Name, model.Note);
    }

    private CodeSaveModel BuildCurrentNoteModel()
    {
        var paths = _treeData?.CurrentPaths.ToArray() ?? [];
        var name = _treeData?.FileName;

        if (string.IsNullOrWhiteSpace(name) && paths.Length > 0)
        {
            name = paths[^1];
            paths = paths.Length > 1 ? paths[..^1] : [];
        }
        else if (!string.IsNullOrWhiteSpace(name) && paths.Length > 0)
        {
            paths = paths[..^1];
        }

        return new CodeSaveModel
        {
            Name = name ?? "",
            Path = paths,
            Note = null,
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

    private static string GetNoteText(string? note)
    {
        return string.IsNullOrWhiteSpace(note) ? "—" : note;
    }

    private string GetFileDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(_treeData?.FileName))
        {
            return _treeData.FileName!;
        }

        return _currentPath.Count == 0 ? "未命名文件" : _currentPath[^1];
    }
}
