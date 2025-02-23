using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Service.Impl;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Friends;

public partial class List
{
    [Inject]
    private IAppOptionService OptionService { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    private IList<FriendModel> _list = new List<FriendModel>();

    private bool _tableLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDataAsync()
    {
        _tableLoading = true;
        _list = await OptionService.GetFriendsAsync();
        _tableLoading = false;
    }

    private async Task DeleteAsync(string id)
    {
        var result = await DialogService.ShowMessageBox(
            "提示",
            "删除后不能恢复，确定删除？",
            yesText: "删除!",
            cancelText: "取消"
        );
        if (result == true)
        {
            await OptionService.DeleteFriendAsync(id);
            await LoadDataAsync();
        }
    }

    private async Task AddAsync()
    {
        var parameters = new DialogParameters
        {
            ["Title"] = "添加友链",
            ["Model"] = new FriendAddModel(),
        };
        var dialog = DialogService.Show<FriendForm>(
            "",
            parameters,
            new DialogOptions { CloseButton = true }
        );
        var result = await dialog.Result;
        if (
            result?.Canceled == false
            && bool.TryParse(result.Data?.ToString() ?? "", out var r)
            && r
        )
        {
            await LoadDataAsync();
        }
    }

    private async Task EditAsync(FriendModel model)
    {
        FriendAddModel editModel = new FriendEditModel
        {
            Id = model.Id,
            Avatar = model.Avatar,
            Description = model.Description,
            Link = model.Link,
            Name = model.Name,
        };
        var parameters = new DialogParameters { ["Title"] = "添加友链", ["Model"] = editModel };
        var dialog = DialogService.Show<FriendForm>(
            "",
            parameters,
            new DialogOptions { CloseButton = true }
        );
        var result = await dialog.Result;
        if (result?.Canceled == false && bool.TryParse(result.Data?.ToString() ?? "", out var r) && r)
        {
            await LoadDataAsync();
        }
    }
}
