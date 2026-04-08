using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class CodeService(IHttpService httpService) : ICodeService
{
    private static CodeNoteTree EmptyTree()
    {
        return new CodeNoteTree
        {
            IsRoot = false,
            IsDirectory = true,
            Directories = [],
            Files = [],
            ParentPaths = [],
            CurrentPaths = [],
            Type = FileSystemType.NoExists,
        };
    }

    public async Task<CodeNoteTree> GetTreeAsync(params string[]? path)
    {
        var parameters = "";
        if (path is not null && path.Length > 0)
        {
            parameters = string.Join("&", path.Select(x => $"path={x}"));
            parameters = "?" + parameters;
        }
        return await httpService.GetAsync<CodeNoteTree>("/api/Code" + parameters) ?? EmptyTree();
    }

    public async Task SaveNoteAsync(CodeSaveModel model)
    {
        await httpService.PostAsync("/api/Code", model);
    }

    public async Task<CodeNoteTree> SearchAsync(string keyword)
    {
        return await httpService.GetAsync<CodeNoteTree>($"/api/Code/search?keyword={keyword}")
            ?? EmptyTree();
    }

    public async Task<IPagedList<CodeFileSystemEntryListResponse>> GetFlatListAsync(
        CodeFlatRequest request
    )
    {
        return await httpService.GetPageAsync<CodeFileSystemEntryListResponse>(
            "/api/Code/flat",
            request.PageIndex,
            request.PageSize,
            request
        );
    }

    public async Task<List<string[]>> GetDirectoriesAsync(params string[]? pathSegments)
    {
        var parameters = "";
        if (pathSegments is not null && pathSegments.Length > 0)
        {
            parameters = string.Join("&", pathSegments.Select(x => $"pathSegments={x}"));
            parameters = "?" + parameters;
        }
        return await httpService.GetAsync<List<string[]>>("/api/Code/directories" + parameters)
            ?? [];
    }

    public async Task<CodeFileSystemEntryResponse?> GetDetailAsync(params string[] pathSegments)
    {
        if (pathSegments == null || pathSegments.Length == 0)
        {
            return null;
        }

        var parameters = string.Join("&", pathSegments.Select(x => $"pathSegments={x}"));
        return await httpService.GetAsync<CodeFileSystemEntryResponse>(
            $"/api/Code/detail?{parameters}"
        );
    }
}
