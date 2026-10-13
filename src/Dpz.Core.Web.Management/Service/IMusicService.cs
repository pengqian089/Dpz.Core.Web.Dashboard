using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Management.Helper;
using Dpz.Core.Web.Management.Models;
using Dpz.Core.Web.Management.Models.Upload;

namespace Dpz.Core.Web.Management.Service;

public interface IMusicService
{
    Task<IPagedList<MusicModel>> GetPageAsync(
        string? title = null,
        int pageIndex = 1,
        int pageSize = 10
    );

    Task EditInformationAsync(MultipartFormDataContent content);

    Task AddMusicAsync(MultipartFormDataContent content);

    Task AddMusicWithProgressAsync(
        IReadOnlyList<UploadFilePart> files,
        IReadOnlyList<UploadFormField>? fields = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<MusicModel?> GetMusicAsync(string id);

    Task DeleteAsync(string id);

    Task<List<string>> GetGroupsAsync();
}
