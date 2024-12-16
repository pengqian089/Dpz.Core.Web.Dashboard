﻿using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Video
{
    [Inject]
    private IVideoService VideoService { get; set; }

    [Inject] private IDialogService DialogService { get; set; }

    private IList<VideoModel> _source = new List<VideoModel>();

    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _source = await VideoService.GetVideosAsync();
        _isLoading = false;
        await base.OnInitializedAsync();
    }

    private VideoModel _beforeEdit;
    private void BackupVideo(object element)
    {
        var json = JsonSerializer.Serialize(element);
        _beforeEdit = JsonSerializer.Deserialize<VideoModel>(json);
    }

    private VideoModel _commitModel;
    private void ItemHasBeenCommitted(object element)
    {
        if (element is VideoModel videoModel)
        {
            videoModel.Tags = string.IsNullOrEmpty(videoModel.TagsValue) ? null : videoModel.TagsValue.Split(",");
            videoModel.CopyTo(out _commitModel);
            _isLoading = true;
        }
        
    }

    private void ResetItemToOriginalValues(object element)
    {
        if (element is VideoModel videoModel)
        {
            videoModel.Name = _beforeEdit.Name;
            videoModel.VideoTitle = _beforeEdit.VideoTitle;
            videoModel.SubTitle = _beforeEdit.SubTitle;
            videoModel.Tags = _beforeEdit.Tags;
            videoModel.Description = _beforeEdit.Description;
        }
    }

    private async Task CommitInformationAsync(MouseEventArgs arg)
    {
        await Task.Delay(100);
        await VideoService.SaveVideoInformationAsync(_commitModel);
        _source = await VideoService.GetVideosAsync();
        _commitModel = null;
        _isLoading = false;
    }

    private void PreviewVideo(string url,string title)
    {
        var parameters = new DialogParameters
        {
            ["VideoUrl"] = url,
            ["Title"] = title
        };
        DialogService.Show<Player>("", parameters, new DialogOptions { CloseButton = true });
    }
}