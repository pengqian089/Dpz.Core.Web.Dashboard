using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Account
{
    public partial class Account
    {
        [Inject]
        private IAccountService AccountService { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        #region query parameter
        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _account = "";

        #endregion

        private MudTable<UserInfo> _table;

        private readonly Func<int, string> _convert = x =>
            Enum.ToObject(typeof(PictureType), x).ToString();

        #region temp

        private string _tempAccount = "";
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _tempAccount = _account;
            await base.OnInitializedAsync();
        }

        private async Task<TableData<UserInfo>> LoadDataAsync(TableState state)
        {
            if (_account == _tempAccount)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempAccount = _account;
            }
            var list = await AccountService.GetPageAsync(_account, _pageIndex, PageSize);
            return new TableData<UserInfo>() { TotalItems = list.TotalItemCount, Items = list };
        }

        private void Search()
        {
            _table.ReloadServerData();
        }

        private async Task CreateAccount()
        {
            var parameters = new DialogParameters
            {
                ["Account"] = "",
                ["Name"] = "",
                ["IsCreate"] = true,
            };
            var dialog = DialogService.Show<AccountForm>(
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
                Search();
            }
        }

        private async Task ChangePassword(string account, string name)
        {
            var parameters = new DialogParameters
            {
                ["Account"] = account,
                ["Name"] = name,
                ["IsCreate"] = false,
            };
            var dialog = DialogService.Show<AccountForm>(
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
                Search();
            }
        }

        private void ViewAvatar(string avatar)
        {
            var parameters = new DialogParameters { ["Avatar"] = avatar };
            DialogService.Show<PreviewAvatar>(
                "",
                parameters,
                new DialogOptions { CloseButton = true }
            );
        }

        private async Task Enable(string account)
        {
            await AccountService.EnableAsync(account);
            Search();
        }

        private async Task ViewTokenHistoryAsync(string account)
        {
            var parameters = new DialogParameters { ["Account"] = account };
            await DialogService.ShowAsync<TokenHistory>(
                "",
                parameters,
                new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.ExtraExtraLarge,
                    BackdropClick = false,
                }
            );
        }
    }
}
