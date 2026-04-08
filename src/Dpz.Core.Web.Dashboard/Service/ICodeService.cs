using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ICodeService
{
    Task<CodeNoteTree> GetTreeAsync(params string[] path);

    Task SaveNoteAsync(CodeSaveModel model);

    Task<CodeNoteTree> SearchAsync(string keyword);

    Task<IPagedList<CodeFileSystemEntryListResponse>> GetFlatListAsync(CodeFlatRequest request);

    Task<List<string[]>> GetDirectoriesAsync(params string[]? pathSegments);

    Task<CodeFileSystemEntryResponse?> GetDetailAsync(params string[] pathSegments);
}
