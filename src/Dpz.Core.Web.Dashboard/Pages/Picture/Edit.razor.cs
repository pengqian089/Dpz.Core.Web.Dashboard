using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages.Picture;

public partial class Edit(
    IAppDialogService appDialogService,
    NavigationManager navigationManager,
    IPictureService pictureService,
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService,
    ILogger<Edit> logger
) : IAsyncDisposable
{
    private bool _editPicture;
    private EditPictureRequest _picture = new() { Id = "" };
    private List<string> _tags = [];
    private IJSObjectReference? _jsModule;
    private bool _pictureLoaded;

    [Parameter]
    public string? Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _tags = await pictureService.GetTagsAsync();
        var modulePath = await assetManifestService.GetAssetPathAsync("src/photoswipe-gallery.ts");
        try
        {
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import photo swipe gallery module.");
        }

        if (!string.IsNullOrWhiteSpace(Id))
        {
            var response = await pictureService.GetPictureAsync(Id);
            if (response != null)
            {
                _picture = new EditPictureRequest
                {
                    Id = response.Id,
                    Tags = response.Tags.ToList(),
                    Description = response.Description,
                    ImageUrl = response.AccessUrl,
                    FileName = response.AccessUrl.Split('/').LastOrDefault() ?? "",
                    Length = response.Length,
                };
                _pictureLoaded = true;
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pictureLoaded && _jsModule != null && !string.IsNullOrWhiteSpace(_picture.ImageUrl))
        {
            _pictureLoaded = false;
            try
            {
                await _jsModule.InvokeVoidAsync("initPhotoSwipe", ".pswp-gallery");
            }
            catch (Exception ex)
            {
                await appDialogService.ShowAlertAsync(
                    new AppDialogOptions
                    {
                        Title = "错误",
                        Message = $"图片查看器初始化失败：{ex.Message}",
                    }
                );
            }
        }
    }

    private async Task EditPictureAsync()
    {
        if (string.IsNullOrWhiteSpace(_picture.Id))
        {
            return;
        }

        try
        {
            _editPicture = true;
            await pictureService.EditAsync(_picture);
            appDialogService.Toast("图像信息更新成功", ToastType.Success);
            navigationManager.NavigateTo("/picture");
        }
        catch (Exception exception)
        {
            appDialogService.Toast(exception.Message, ToastType.Error);
        }
        finally
        {
            _editPicture = false;
        }
    }

    private void HandleNewTagAdded(string tag)
    {
        appDialogService.Toast($"标签 '{tag}' 已添加", ToastType.Success);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("destroyPhotoViewer");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to destroy photo viewer.");
            }
            await _jsModule.DisposeAsync();
        }
    }
}
