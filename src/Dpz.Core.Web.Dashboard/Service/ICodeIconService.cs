using System.Threading;
using System.Threading.Tasks;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ICodeIconService
{
    Task<string> GetIconUrlAsync(
        string? name,
        bool isFolder,
        bool isSubmodule = false,
        bool isSymlink = false,
        CancellationToken cancellationToken = default
    );
}
