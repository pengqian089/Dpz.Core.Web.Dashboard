using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IAudioService
    {
        Task<IPagedList<AudioModel>> GetPageAsync(int pageIndex, int pageSize);

        Task DeleteAsync(string id);
    }
}