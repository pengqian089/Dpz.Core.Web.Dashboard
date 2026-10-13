using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Management.Helper;
using Dpz.Core.Web.Management.Models;

namespace Dpz.Core.Web.Management.Service;

public interface ITimelineService
{
    Task<IPagedList<TimelineModel>> GetPageAsync(
        string? title = null,
        int pageIndex = 1,
        int pageSize = 10
    );

    Task CreateTimelineAsync(TimelineCreateRequest request);

    Task EditTimelineAsync(TimelineEditRequest request);

    Task<TimelineModel?> GetTimelineAsync(string id);

    Task DeleteAsync(string id);

    Task<string?> UploadAsync(MultipartFormDataContent content);
}
