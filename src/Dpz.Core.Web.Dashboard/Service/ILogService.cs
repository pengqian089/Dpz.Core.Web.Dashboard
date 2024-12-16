using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ILogService
{
    Task<IPagedList<LogModel>> GetPageAsync(
        string logger = null, 
        string level = null, 
        string message = null, 
        int pageIndex = 1,
        int pageSize = 15);

    Task<IList<string>> GetLoggerAsync();

    Task<IList<string>> GetLevelAsync();

    Task<string> GetDetailAsync(string id);
}