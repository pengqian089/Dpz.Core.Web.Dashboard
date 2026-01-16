using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages;

public partial class Footer(ICommunityService communityService, IAppDialogService dialogService)
{
    private readonly object _formState = new();
    private bool _isLoading = true;
    private bool _isSaving;
    private string _content = "";
    private HtmlEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            _content = await communityService.GetFooterAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SaveAsync(EditContext context)
    {
        if (_editor == null)
        {
            dialogService.Toast("编辑器未初始化", ToastType.Error);
            return;
        }

        _isSaving = true;
        StateHasChanged();
        try
        {
            var content = await _editor.GetValueAsync();
            await communityService.SaveFooterAsync(content);
            dialogService.Toast("保存成功", ToastType.Success);
            _content = content;
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
}
