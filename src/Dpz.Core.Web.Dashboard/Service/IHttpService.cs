using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Upload;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IHttpService
{
    Task<T?> GetAsync<T>(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task<IPagedList<T>> GetPageAsync<T>(
        string uri,
        int pageIndex = 1,
        int pageSize = 10,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task<T?> PostAsync<T>(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task PostAsync(string uri, object? value = null, CancellationToken cancellationToken = default);

    Task<T?> PutAsync<T>(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task PutAsync(string uri, object? value = null, CancellationToken cancellationToken = default);

    Task<T?> PatchAsync<T>(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task PatchAsync(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task<T?> DeleteAsync<T>(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        string uri,
        object? value = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <param name="content"></param>
    /// <param name="method">默认POST</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> PostFileAsync<T>(
        string uri,
        MultipartFormDataContent content,
        HttpMethod? method = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="content"></param>
    /// <param name="method">默认POST</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PostFileAsync(
        string uri,
        MultipartFormDataContent content,
        HttpMethod? method = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// 上传文件(带进度)
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="files"></param>
    /// <param name="fields"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    Task PostFileWithProgressAsync(
        string uri,
        IReadOnlyList<UploadFilePart> files,
        IReadOnlyList<UploadFormField>? fields = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// 上传文件(带进度)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <param name="files"></param>
    /// <param name="fields"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> PostFileWithProgressAsync<T>(
        string uri,
        IReadOnlyList<UploadFilePart> files,
        IReadOnlyList<UploadFormField>? fields = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default
    );
}
