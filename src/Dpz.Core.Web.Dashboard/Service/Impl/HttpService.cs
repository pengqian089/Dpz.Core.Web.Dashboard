using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class HttpService : IHttpService
    {
        private readonly ILogger<HttpService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authenticationService;
        private readonly NavigationManager _navigation;
        private readonly ILocalStorageService _localStorageService;

        //private static readonly Dictionary<string, string> CacheHttpHeaders = new();

        /// <summary>
        /// 缓存数据
        /// </summary>
        private static readonly Dictionary<string, object> CacheResponseData = new();

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);

        public HttpService(
            ILogger<HttpService> logger,
            HttpClient httpClient,
            IAuthenticationService authenticationService,
            NavigationManager navigation,
            ILocalStorageService localStorageService
        )
        {
            _logger = logger;
            _httpClient = httpClient;
            _authenticationService = authenticationService;
            _navigation = navigation;
            _localStorageService = localStorageService;
        }

        private async Task SendRequestAsync(HttpRequestMessage request)
        {
            await SemaphoreSlim.WaitAsync();
            var user = _authenticationService.User;
            try
            {
                // 没有获取到当前登录用户
                if (user == null)
                {
                    _navigation.NavigateTo("/login");
                    return;
                }
                // Token过期
                if (DateTime.Now > user.Expires)
                {
                    var result = await _authenticationService.RefreshTokenAsync(
                        user.Token,
                        user.RefreshToken
                    );
                    if (!result)
                    {
                        _navigation.NavigateTo("/login");
                        return;
                    }
                    user = await _localStorageService.GetItemAsync<AppUser>("Identity");
                }
                // ETag
                var eTagKey = $"{request.RequestUri}|{request.Method}|ETag";
                // if (CacheHttpHeaders.ContainsKey(eTagKey))
                // {
                //     request.Headers.Add("If-None-Match", CacheHttpHeaders[eTagKey]);
                // }
                // // Last-Modified
                // var lastModifiedKey = $"{request.RequestUri}|{request.Method}|Last-Modified";
                // if (CacheHttpHeaders.ContainsKey(lastModifiedKey))
                // {
                //     request.Headers.Add("If-Modified-Since", CacheHttpHeaders[lastModifiedKey]);
                // }
                // 身份认证
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
                // 开始请求API
                using var response = await _httpClient.SendAsync(request);
                // if (response.IsSuccessStatusCode)
                // {
                //     CacheHttpHeaders[eTagKey] = response.Headers.ETag?.ToString();
                //     var culture = new CultureInfo("en-US");
                //     CacheHttpHeaders[lastModifiedKey] =
                //         response.Content.Headers.LastModified?.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", culture);
                // }

                // 如果响应401，就返回登录页面
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _navigation.NavigateTo("/login");
                    return;
                }
                // 如果请求不成功并且不是缓存响应则抛出异常
                if (
                    !response.IsSuccessStatusCode
                    && response.StatusCode != HttpStatusCode.NotModified
                )
                {
                    // var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    // _logger.LogDebug("request error:{Error}", error);
                    var result = await response.Content.ReadAsStringAsync();
                    throw new FetchException(result);
                }
            }
            finally
            {
                _logger.LogDebug(
                    "current user:{User}",
                    JsonSerializer.Serialize(
                        user,
                        new JsonSerializerOptions { WriteIndented = true }
                    )
                );
                SemaphoreSlim.Release();
            }
        }

        private static readonly JsonSerializerOptions JsonSerializerOptions =
            new() { PropertyNameCaseInsensitive = true };

        private async Task<T> SendRequestAsync<T>(
            HttpRequestMessage request,
            Action<HttpResponseMessage> action = null
        )
        {
            await SemaphoreSlim.WaitAsync();
            var user = _authenticationService.User;
            try
            {
                // 没有获取到当前登录用户
                if (user == null)
                {
                    _navigation.NavigateTo("/login");
                    return default;
                }

                // Token过期
                if (DateTime.Now > user.Expires)
                {
                    var result = await _authenticationService.RefreshTokenAsync(
                        user.Token,
                        user.RefreshToken
                    );
                    if (!result)
                    {
                        _navigation.NavigateTo("/login");
                        return default;
                    }

                    user = await _localStorageService.GetItemAsync<AppUser>("Identity");
                }

                // ETag
                var eTagKey = $"{request.RequestUri}|{request.Method}|ETag";
                // if (CacheHttpHeaders.ContainsKey(eTagKey))
                // {
                //     request.Headers.Add("If-None-Match", CacheHttpHeaders[eTagKey]);
                // }
                // // Last-Modified
                // var lastModifiedKey = $"{request.RequestUri}|{request.Method}|Last-Modified";
                // if (CacheHttpHeaders.ContainsKey(lastModifiedKey))
                // {
                //     request.Headers.Add("If-Modified-Since", CacheHttpHeaders[lastModifiedKey]);
                // }
                // 身份认证
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                };
                // 开始请求API
                using var response = await _httpClient.SendAsync(request);
                // if (response.IsSuccessStatusCode)
                // {
                //     CacheHttpHeaders[eTagKey] = response.Headers.ETag?.ToString();
                //     var culture = new CultureInfo("en-US");
                //     CacheHttpHeaders[lastModifiedKey] =
                //         response.Content.Headers.LastModified?.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", culture);
                // }
                // 如果响应401，就返回登录页面
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _navigation.NavigateTo("/login");
                    return default;
                }

                // 如果请求不成功并且不是缓存响应则抛出异常
                if (
                    !response.IsSuccessStatusCode
                    && response.StatusCode != HttpStatusCode.NotModified
                )
                {
                    var error = await response.Content.ReadFromJsonAsync<
                        Dictionary<string, string>
                    >();
                    _logger.LogDebug("request error:{Error}", error);
                }

                // 处理304缓存
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    if (CacheResponseData.ContainsKey(eTagKey))
                    {
                        return (T)CacheResponseData[eTagKey];
                    }

                    return default;
                }

                if (action != null)
                    action(response);

                if (typeof(T) == typeof(string))
                {
                    object content = await response.Content.ReadAsStringAsync();
                    CacheResponseData[eTagKey] = content;
                    return (T)content;
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default;
                }

                // var responseData = await response.Content.ReadFromJsonAsync<T>(
                //     new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                // );
                var json = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);

                CacheResponseData[eTagKey] = responseData;
                return responseData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default;
            }
            finally
            {
                _logger.LogDebug(
                    "current user:{User}",
                    JsonSerializer.Serialize(
                        user,
                        new JsonSerializerOptions { WriteIndented = true }
                    )
                );
                SemaphoreSlim.Release();
            }
        }

        private string HandleParameter(string uri, object value)
        {
            if (value == null)
                return uri;
            var index = uri.IndexOf("?", StringComparison.CurrentCultureIgnoreCase);
            var query = index >= 0 ? uri.Substring(index) : "";
            var queryParameters = HttpUtility.ParseQueryString(query);

            var properties = value.GetType().GetProperties();
            foreach (var property in properties)
            {
                queryParameters.Add(property.Name, property.GetValue(value)?.ToString());
            }

            var newUri = "";
            if (index >= 0)
            {
                newUri = uri.Substring(0, index + 1) + queryParameters;
            }
            else
            {
                newUri += "?" + queryParameters;
            }

            return newUri;
        }

        public async Task<T> GetAsync<T>(string uri, object value = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, HandleParameter(uri, value));
            return await SendRequestAsync<T>(request);
        }

        public async Task<IPagedList<T>> GetPageAsync<T>(
            string uri,
            int pageIndex = 1,
            int pageSize = 10,
            object value = null
        )
        {
            var pageUri = $"{uri}?pageIndex={pageIndex}&pageSize={pageSize}";
            var request = new HttpRequestMessage(HttpMethod.Get, HandleParameter(pageUri, value));
            var pagination = new Pagination();
            var list = await SendRequestAsync<List<T>>(
                request,
                x =>
                {
                    var xPagination = x.Headers.GetValues("X-Pagination").FirstOrDefault();
                    if (xPagination != null)
                    {
                        pagination = JsonSerializer.Deserialize<Pagination>(
                            xPagination,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                    }
                }
            );
            var pagedList = new PagedList<T>(
                list ?? new List<T>(),
                pagination.CurrentPage,
                pagination.PageSize,
                pagination.TotalCount
            );
            return pagedList;
        }

        public async Task<T> PostAsync<T>(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            if (value != null)
                request.Content = JsonContent.Create(value);
            return await SendRequestAsync<T>(request);
        }

        public async Task PostAsync(string uri, object value)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            if (value != null)
            {
                SetHttpContent(value, request);
            }
            await SendRequestAsync(request);
        }

        private static void SetHttpContent(object value, HttpRequestMessage request)
        {
            HttpContent httpContent;
            if (value is string content)
            {
                httpContent = new StringContent(content, Encoding.UTF8);
            }
            else
            {
                httpContent = JsonContent.Create(value);
            }
            request.Content = httpContent;
        }

        public async Task<T> PutAsync<T>(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            if (value != null)
                request.Content = JsonContent.Create(value);
            return await SendRequestAsync<T>(request);
        }

        public async Task PutAsync(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            if (value != null)
                request.Content = JsonContent.Create(value);
            await SendRequestAsync(request);
        }

        public async Task<T> PatchAsync<T>(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, uri);
            if (value != null)
                SetHttpContent(value, request);
            return await SendRequestAsync<T>(request);
        }

        public async Task PatchAsync(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, uri);
            if (value != null)
                SetHttpContent(value, request);
            await SendRequestAsync(request);
        }

        public async Task<T> DeleteAsync<T>(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            if (value != null)
                request.Content = JsonContent.Create(value);
            return await SendRequestAsync<T>(request);
        }

        public async Task DeleteAsync(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            if (value != null)
                request.Content = JsonContent.Create(value);
            await SendRequestAsync(request);
        }

        public async Task<T> PostFileAsync<T>(
            string uri,
            MultipartFormDataContent content,
            HttpMethod method = null
        )
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Post, uri);
            request.Content = content;
            return await SendRequestAsync<T>(request);
        }

        public async Task PostFileAsync(
            string uri,
            MultipartFormDataContent content,
            HttpMethod method = null
        )
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Post, uri);
            request.Content = content;
            await SendRequestAsync(request);
        }
    }
}
