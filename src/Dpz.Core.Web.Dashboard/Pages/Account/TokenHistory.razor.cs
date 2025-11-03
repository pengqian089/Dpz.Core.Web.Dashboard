using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Account;

public partial class TokenHistory : ComponentBase
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    [Inject]
    private IAccountService AccountService { get; set; }

    [Parameter, Required]
    public string Account { get; set; }

    [Parameter]
    public bool? Used { get; set; }

    private bool _isLoading = true;

    #region query parameter
    private int _pageIndex = 1;

    private const int PageSize = 10;

    #endregion

    private MudTable<AccountTokenResponse> _table;

    private async Task<TableData<AccountTokenResponse>> LoadDataAsync(TableState state)
    {
        _isLoading = true;
        _pageIndex = state.Page + 1;
        Console.WriteLine($"PageIndex:{_pageIndex}");
        var list = await AccountService.GetTokenHistoryAsync(Account, Used, _pageIndex, PageSize);
        _isLoading = false;
        Console.WriteLine($"IsLoading:{_isLoading}");
        return new TableData<AccountTokenResponse>()
        {
            TotalItems = list.TotalItemCount,
            Items = list,
        };
    }

    private void ShowToken(AccountTokenResponse context)
    {
        if (context.ShowToken)
        {
            context.ShowTokenIcon = Icons.Material.Filled.Visibility;
        }
        else
        {
            context.ShowTokenIcon = Icons.Material.Filled.VisibilityOff;
        }
        context.ShowToken = !context.ShowToken;
    }

    private void Close()
    {
        MudDialog.Cancel();
    }
}
