using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Account;

public partial class Account(
    IAccountService accountService,
    IAppDialogService dialogService,
    NavigationManager navigation
) : ComponentBase
{
    private int _pageIndex = 1;
    private int _totalPages;
    private int _totalCount;
    private const int PageSize = 12;

    private string _account = "";
    private List<UserInfo> _accounts = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        ReadQueryParameters();
        await LoadDataAsync();
    }

    private void ReadQueryParameters()
    {
        var uri = new Uri(navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (int.TryParse(query["page"], out var page) && page > 0)
        {
            _pageIndex = page;
        }

        _account = query["account"] ?? "";
    }

    private void UpdateUrl()
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = _pageIndex > 1 ? _pageIndex.ToString() : null,
            ["account"] = !string.IsNullOrWhiteSpace(_account) ? _account : null,
        };

        var filteredParams = queryParams
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}");

        var queryString = string.Join("&", filteredParams);
        var newUrl = string.IsNullOrEmpty(queryString) ? "/account" : $"/account?{queryString}";

        navigation.NavigateTo(newUrl, forceLoad: false);
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        var list = await accountService.GetPageAsync(_account, _pageIndex, PageSize);
        _accounts = list.ToList();
        _totalCount = list.TotalItemCount;
        _totalPages = list.TotalPageCount;

        _isLoading = false;
        StateHasChanged();
    }

    private async Task Search()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task HandlePageChanged(int newPage)
    {
        _pageIndex = newPage;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Search();
        }
    }

    private async Task Reload()
    {
        await LoadDataAsync();
    }

    private async Task CreateAccount()
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "创建账号",
            BuildAccountForm("", "", true),
            "520px"
        );

        if (result == true)
        {
            await LoadDataAsync();
        }
    }

    private async Task ChangePassword(string account, string name)
    {
        var result = await dialogService.ShowComponentAsync<bool>(
            "修改密码",
            BuildAccountForm(account, name, false),
            "520px"
        );

        if (result == true)
        {
            await LoadDataAsync();
        }
    }

    private async Task ViewAvatar(string avatar)
    {
        if (string.IsNullOrWhiteSpace(avatar))
        {
            dialogService.Toast("当前账号未设置头像", ToastType.Warning);
            return;
        }

        await dialogService.ShowComponentAsync(
            "查看头像",
            builder =>
            {
                builder.OpenComponent<PreviewAvatar>(0);
                builder.AddAttribute(1, "Avatar", avatar);
                builder.CloseComponent();
            },
            "600px"
        );
    }

    private async Task ToggleEnableAsync(UserInfo account)
    {
        if (account.Permissions == Permissions.System)
        {
            dialogService.Toast("System 账号不可操作", ToastType.Warning);
            return;
        }

        var actionText = IsEnabled(account) ? "停用" : "启用";
        var confirmed = await dialogService.ConfirmAsync(
            $"确定要{actionText}该账号吗？",
            "确认操作"
        );

        if (!confirmed)
        {
            return;
        }

        await accountService.EnableAsync(account.Id);
        dialogService.Toast("账号状态已更新", ToastType.Success);
        await LoadDataAsync();
    }

    private async Task ViewTokenHistoryAsync(string account)
    {
        await dialogService.ShowComponentAsync(
            "登录记录",
            builder =>
            {
                builder.OpenComponent<TokenHistory>(0);
                builder.AddAttribute(1, "Account", account);
                builder.CloseComponent();
            },
            "1100px"
        );
    }

    private static RenderFragment BuildAccountForm(string account, string name, bool isCreate)
    {
        return builder =>
        {
            builder.OpenComponent<AccountForm>(0);
            builder.AddAttribute(1, "Account", account);
            builder.AddAttribute(2, "Name", name);
            builder.AddAttribute(3, "IsCreate", isCreate);
            builder.CloseComponent();
        };
    }

    private static string GetSexText(Sex sex)
    {
        return sex == Sex.Wuman ? "女" : "男";
    }

    private static string GetPermissionText(Permissions? permissions)
    {
        return permissions?.ToString() ?? "未设置";
    }

    private static string GetEnableText(bool? enable)
    {
        return enable switch
        {
            true => "已启用",
            false => "已停用",
            _ => "未知",
        };
    }

    private static string GetEnableBadgeClass(bool? enable)
    {
        return enable switch
        {
            true => "account-table__badge--enabled",
            false => "account-table__badge--disabled",
            _ => "account-table__badge--unknown",
        };
    }

    private static string GetLastAccessText(DateTime? accessTime)
    {
        return accessTime?.ToString("yyyy-MM-dd HH:mm") ?? "暂无记录";
    }

    private static string GetAvatarInitial(UserInfo account)
    {
        var text = string.IsNullOrWhiteSpace(account.Name) ? account.Id : account.Name;
        if (string.IsNullOrWhiteSpace(text))
        {
            return "?";
        }

        return text.Trim()[0].ToString();
    }

    private static bool IsEnabled(UserInfo account)
    {
        return account.Enable != false;
    }

}
