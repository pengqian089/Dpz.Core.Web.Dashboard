using System.Threading.Tasks;
using Dpz.Core.Web.Management.Helper;
using Dpz.Core.Web.Management.Models;

namespace Dpz.Core.Web.Management.Service;

public interface IAudioService
{
    Task<IPagedList<AudioModel>> GetPageAsync(int pageIndex, int pageSize);

    Task DeleteAsync(string id);
}
