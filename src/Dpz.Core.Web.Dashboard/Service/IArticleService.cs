using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IArticleService
    {
        Task<IPagedList<ArticleModel>> GetPageAsync(int pageIndex, int pageSize, string tag, string title);

        Task PublishAsync(ArticlePublishRequest request);

        Task<ArticleModel> GetArticleAsync(string id);

        Task<List<string>> GetTagsAsync();

        Task<List<ArticleModel>> GetNewArticlesAsync();

        Task<List<ArticleModel>> GetTopViewArticlesAsync();

        Task EditAsync(ArticleEditRequest request);

        Task DeleteAsync(string id);
        
        Task<bool> ExistsAsync(string title);

        Task<string> UploadAsync(MultipartFormDataContent content);
    }
}