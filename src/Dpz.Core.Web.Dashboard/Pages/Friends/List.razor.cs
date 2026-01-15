using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Friends;

public partial class List(IAppOptionService optionService, IAppDialogService dialogService)
{
    private List<FriendModel> _friends = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();
        _friends = (await optionService.GetFriendsAsync()).ToList();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task DeleteAsync(string id)
    {
        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (confirmed)
        {
            await optionService.DeleteFriendAsync(id);
            dialogService.Toast("删除成功", ToastType.Success);
            await LoadDataAsync();
        }
    }

    private async Task AddAsync()
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "新增友链",
            BuildFriendForm(new FriendAddModel()),
            "720px"
        );

        if (result)
        {
            await LoadDataAsync();
        }
    }

    private async Task EditAsync(FriendModel model)
    {
        var editModel = new FriendEditModel
        {
            Id = model.Id,
            Avatar = model.Avatar,
            Description = model.Description,
            Link = model.Link,
            Name = model.Name,
        };

        var result = await dialogService.ShowComponentAsync<bool>(
            "编辑友链",
            BuildFriendForm(editModel),
            "720px"
        );

        if (result)
        {
            await LoadDataAsync();
        }
    }

    private static string GetAvatarInitial(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        return name.Trim()[0].ToString().ToUpperInvariant();
    }

    private static RenderFragment BuildFriendForm(FriendAddModel model)
    {
        return builder =>
        {
            builder.OpenComponent<FriendForm>(0);
            builder.AddAttribute(1, "Model", model);
            builder.CloseComponent();
        };
    }
}
