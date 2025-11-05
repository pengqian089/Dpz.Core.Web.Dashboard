using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IMusicService
    {
        Task<IPagedList<MusicModel>> GetPageAsync(string title = null, int pageIndex = 1, int pageSize = 10);

        [Obsolete]
        Task EditLyricAsync(MultipartFormDataContent content);

        Task EditInformationAsync(MultipartFormDataContent content);

        Task AddMusicAsync(MultipartFormDataContent content);

        Task<MusicModel> GetMusicAsync(string id);

        Task DeleteAsync(string id);

        [Obsolete("弃用")]
        Task<string> GetLyricAsync(string id);

        Task<List<string>> GetGroupsAsync();
    }
}