using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ICodeService
{
    Task<CodeNoteTree> GetTreeAsync(params string[] path);

    Task SaveNoteAsync(CodeSaveModel model);
    
    Task<CodeNoteTree> SearchAsync(string keyword);
}