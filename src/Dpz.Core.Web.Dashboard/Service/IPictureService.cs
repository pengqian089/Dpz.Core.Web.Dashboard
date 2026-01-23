using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Upload;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IPictureService
{
    /// <summary>
    /// 分页获取图像信息
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="description"></param>
    /// <param name="type"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<IPagedList<PictureResponseModel>> GetPageAsync(
        string? tag = null,
        string? description = null,
        int type = -1,
        int pageIndex = 1,
        int pageSize = 10
    );

    /// <summary>
    /// 获取图像信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<PictureResponseModel?> GetPictureAsync(string? id);

    /// <summary>
    /// 上传图像和相关信息
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task UploadAsync(MultipartFormDataContent content);

    /// <summary>
    /// 上传图像和相关信息(带进度)
    /// </summary>
    /// <param name="files"></param>
    /// <param name="fields"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UploadWithProgressAsync(
        IReadOnlyList<UploadFilePart> files,
        IReadOnlyList<UploadFormField>? fields = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// 上传图像和相关信息
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task EditAsync(EditPictureRequest content);

    /// <summary>
    /// 删除图像和相关信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(string id);

    /// <summary>
    /// 获取所有标签
    /// </summary>
    /// <returns></returns>
    Task<List<string>> GetTagsAsync();
}
