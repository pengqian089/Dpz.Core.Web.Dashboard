using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Friends;

public partial class FriendForm
{
    [Parameter] public string Title { get; set; }
    
    [Parameter] public FriendAddModel Model { get; set; }
    
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }
    
    [Inject] IDialogService DialogService { get; set; }
    
    [Inject] private IAppOptionService OptionService { get; set; }

    private bool _loading = false;

    protected override void OnInitialized()
    {
        Console.WriteLine(Model.GetType());
        Console.WriteLine(JsonSerializer.Serialize(Model));
        base.OnInitialized();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
    
    private async Task SaveAsync()
    {
        _loading = true;

        if (!await CheckInput())
        {
            _loading = false;
            return;
        }
        
        if (Model is FriendEditModel editModel && !string.IsNullOrEmpty(editModel.Id))
        {
            await OptionService.EditFriendAsync(editModel);
        }
        else
        {
            await OptionService.AddFriendAsync(Model);
        }
        MudDialog.Close(DialogResult.Ok(true));
        _loading = false;
    }

    private async Task<bool> CheckInput()
    {
        if (string.IsNullOrEmpty(Model?.Name))
        {
            await DialogService.ShowMessageBox(
                "提示",
                "请输入友链名称！",
                yesText: "确定", cancelText: "取消");
            return false;
        }
        if (string.IsNullOrEmpty(Model?.Link))
        {
            await DialogService.ShowMessageBox(
                "提示",
                "请输入友情链接！",
                yesText: "确定", cancelText: "取消");
            return false;
        }
        if (string.IsNullOrEmpty(Model?.Avatar))
        {
            await DialogService.ShowMessageBox(
                "提示",
                "请输入友链图标！",
                yesText: "确定", cancelText: "取消");
            return false;
        }
        return true;
    }
}