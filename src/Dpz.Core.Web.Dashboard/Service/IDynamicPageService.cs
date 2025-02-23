using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IDynamicPageService
    {
        Task<IPagedList<DynamicPageListModel>> GetPageAsync(string name = null, int pageIndex = 1, int pageSize = 10);

        Task<DynamicPageModel> GetDynamicPageAsync(string id);

        Task<bool> ExistsAsync(string id);

        Task CreateDynamicPage(string id, string htmlContent);
        
        Task EditDynamicPage(string id, string htmlContent);

        Task DeleteAsync(string id);
    }
}