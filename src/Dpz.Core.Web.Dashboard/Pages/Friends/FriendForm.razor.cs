using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Friends;

public partial class FriendForm(IAppOptionService optionService, IAppDialogService dialogService)
{
    [Parameter]
    public FriendAddModel Model { get; set; } = new();

    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    private string _name = "";
    private string _avatar = "";
    private string _link = "";
    private string _description = "";
    private bool _isSubmitting;
    private bool _isInitialized;

    private bool IsEdit => Model is FriendEditModel;
    private string FormTitle => IsEdit ? "编辑友链" : "新增友链";
    private string PreviewName => string.IsNullOrWhiteSpace(_name) ? "站点名称" : _name;
    private string PreviewDescription =>
        string.IsNullOrWhiteSpace(_description) ? "友链描述将在这里展示" : _description;

    protected override void OnParametersSet()
    {
        if (!_isInitialized)
        {
            _name = Model.Name ?? "";
            _avatar = Model.Avatar ?? "";
            _link = Model.Link ?? "";
            _description = Model.Description ?? "";
            _isInitialized = true;
        }
    }

    private void Cancel()
    {
        CloseDialog?.Invoke(null);
    }
    
    private async Task SaveAsync()
    {
        if (!ValidateInput())
        {
            return;
        }
        _isSubmitting = true;

        try
        {
            if (Model is FriendEditModel editModel && !string.IsNullOrWhiteSpace(editModel.Id))
            {
                var payload = new FriendEditModel
                {
                    Id = editModel.Id,
                    Name = _name.Trim(),
                    Avatar = _avatar.Trim(),
                    Link = _link.Trim(),
                    Description = _description.Trim(),
                };
                await optionService.EditFriendAsync(payload);
            }
            else
            {
                var payload = new FriendAddModel
                {
                    Name = _name.Trim(),
                    Avatar = _avatar.Trim(),
                    Link = _link.Trim(),
                    Description = _description.Trim(),
                };
                await optionService.AddFriendAsync(payload);
            }

            dialogService.Toast("保存成功", ToastType.Success);
            CloseDialog?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            dialogService.Toast("请输入友链名称", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_link))
        {
            dialogService.Toast("请输入友情链接地址", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_avatar))
        {
            dialogService.Toast("请输入友链图标地址", ToastType.Warning);
            return false;
        }

        return true;
    }

    private static string GetAvatarInitial(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        return name.Trim()[0].ToString().ToUpperInvariant();
    }
}