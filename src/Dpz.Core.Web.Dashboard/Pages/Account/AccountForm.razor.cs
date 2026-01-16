using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Account;

public partial class AccountForm(IAccountService accountService, IAppDialogService dialogService)
    : ComponentBase
{
    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    [Parameter]
    public string Account { get; set; } = "";

    [Parameter]
    public string Name { get; set; } = "";

    [Parameter]
    public bool IsCreate { get; set; }

    private string _accountValue = "";
    private string _nameValue = "";
    private string _password = "";
    private bool _isSubmitting;
    private bool _isPasswordVisible;
    private bool _isInitialized;

    private string PasswordInputType => _isPasswordVisible ? "text" : "password";
    private string PasswordToggleIcon => _isPasswordVisible ? "fa-eye" : "fa-eye-slash";

    protected override void OnParametersSet()
    {
        if (!_isInitialized || !IsCreate)
        {
            _accountValue = Account ?? "";
            _nameValue = Name ?? "";
            _isInitialized = true;
        }
    }

    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
    }

    private void Cancel()
    {
        CloseDialog?.Invoke(null);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SaveAsync();
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_accountValue))
        {
            dialogService.Toast("请输入账号", ToastType.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameValue))
        {
            dialogService.Toast("请输入昵称", ToastType.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_password))
        {
            dialogService.Toast("请输入密码", ToastType.Warning);
            return;
        }

        _isSubmitting = true;

        try
        {
            if (IsCreate)
            {
                if (await accountService.ExistsAsync(_accountValue))
                {
                    dialogService.Toast("该账号已存在", ToastType.Warning);
                    _isSubmitting = false;
                    return;
                }

                await accountService.CreateAccountAsync(_accountValue, _nameValue, _password);
                dialogService.Toast("账号创建成功", ToastType.Success);
            }
            else
            {
                await accountService.ChangePasswordAsync(_accountValue, _password);
                dialogService.Toast("密码修改成功", ToastType.Success);
            }

            CloseDialog?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"操作失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
