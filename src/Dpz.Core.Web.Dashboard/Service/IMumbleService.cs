using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IMumbleService
    {
        Task<IPagedList<MumbleModel>> GetPageAsync(string content = null, int pageIndex = 1, int pageSize = 10);

        Task CreateAsync(string markdown,string htmlContent);

        Task EditAsync(string id, string markdown, string htmlContent);

        Task<MumbleModel> GetMumbleAsync(string id);

        Task DeleteAsync(string id);

        Task DeleteCommentAsync(string id,string commentId);

        Task<IPagedList<MumbleCommentModel>> GetCommentPageAsync(string id, string content = null, int pageIndex = 1,
            int pageSize = 10);

        Task<string> UploadAsync(MultipartFormDataContent content);
    }
}