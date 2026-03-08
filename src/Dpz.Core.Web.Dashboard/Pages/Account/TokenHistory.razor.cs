using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Account;

public partial class TokenHistory(IAccountService accountService) : ComponentBase
{
    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    [Parameter, Required]
    public string Account { get; set; } = "";

    private bool _isLoading = true;
    private int _pageIndex = 1;
    private int _totalPages;
    private int _totalCount;
    private const int PageSize = 10;
    private List<AccountLoginHistoryResponse> _records = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        var request = new AccountLoginHistoryRequest { Account = Account };
        var list = await accountService.GetAccountLoginHistoryAsync(request, _pageIndex, PageSize);

        _records = list.ToList();
        _totalCount = list.TotalItemCount;
        _totalPages = list.TotalPageCount;

        _isLoading = false;
        StateHasChanged();
    }

    private async Task HandlePageChanged(int newPage)
    {
        _pageIndex = newPage;
        await LoadDataAsync();
    }

    private void Close()
    {
        CloseDialog?.Invoke(null);
    }

    private static string GetStatusClass(LoginResultStatus status)
    {
        return status switch
        {
            LoginResultStatus.Success => "account-token__status--success",
            LoginResultStatus.AccountOrPasswordError => "account-token__status--danger",
            LoginResultStatus.PinCodeError => "account-token__status--warning",
            LoginResultStatus.AccountLocked => "account-token__status--info",
            _ => "account-token__status--info",
        };
    }
}
