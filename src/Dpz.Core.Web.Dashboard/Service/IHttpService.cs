using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string uri, object value = null);

        Task<IPagedList<T>> GetPageAsync<T>(string uri, int pageIndex = 1, int pageSize = 10, object value = null);

        Task<T> PostAsync<T>(string uri, object value = null);

        Task PostAsync(string uri, object value = null);

        Task<T> PutAsync<T>(string uri, object value = null);

        Task PutAsync(string uri, object value = null);

        Task<T> PatchAsync<T>(string uri, object value = null);

        Task PatchAsync(string uri, object value = null);

        Task<T> DeleteAsync<T>(string uri, object value = null);

        Task DeleteAsync(string uri, object value = null);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="method">默认POST</param>
        /// <returns></returns>
        Task<T> PostFileAsync<T>(string uri, MultipartFormDataContent content, HttpMethod method = null);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="method">默认POST</param>
        /// <returns></returns>
        Task PostFileAsync(string uri, MultipartFormDataContent content, HttpMethod method = null);
    }
}