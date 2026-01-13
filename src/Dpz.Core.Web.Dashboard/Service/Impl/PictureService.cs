using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Pages.Article;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class PictureService(IHttpService httpService) : IPictureService
{
    public async Task<IPagedList<PictureResponseModel>> GetPageAsync(
        string? tag = null,
        string? description = null,
        int type = -1,
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        return await httpService.GetPageAsync<PictureResponseModel>(
            "/api/Picture",
            pageIndex,
            pageSize,
            new
            {
                tag,
                description,
                type,
            }
        );
    }

    public async Task<PictureResponseModel?> GetPictureAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }
        return await httpService.GetAsync<PictureResponseModel>($"/api/Picture/{id}");
    }

    public async Task UploadAsync(MultipartFormDataContent content)
    {
        await httpService.PostFileAsync("/api/Picture", content);
    }

    public async Task EditAsync(EditPictureRequest content)
    {
        await httpService.PatchAsync("/api/Picture", content);
    }

    public async Task DeleteAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Picture/{id}");
    }

    public async Task<List<string>> GetTagsAsync()
    {
        return await httpService.GetAsync<List<string>>("/api/Picture/tags");
    }
}
