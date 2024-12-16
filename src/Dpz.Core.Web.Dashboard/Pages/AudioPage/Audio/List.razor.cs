using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Audio
{
    public partial class List
    {
        [Inject] private IAudioService AudioService { get; set; }

        [Inject] private IDialogService DialogService { get; set; }

        #region query parameter
        private int _pageIndex = 1;

        private const int PageSize = 12;

        #endregion

        private MudTable<AudioModel> _table;

        private async Task<TableData<AudioModel>> LoadDataAsync(TableState state)
        {
            _pageIndex = state.Page + 1;
            var list = await AudioService.GetPageAsync( _pageIndex, PageSize);
            return new TableData<AudioModel>()
            {
                TotalItems = list.TotalItemCount,
                Items = list
            };
        }

        private void Search()
        {
            _table.ReloadServerData();
        }

        private async Task DeleteAsync(string id)
        {
            var result = await DialogService.ShowMessageBox(
                "提示",
                "删除后不能恢复，确定删除？",
                yesText: "删除!", cancelText: "取消");
            if (result == true)
            {
                await AudioService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }
    }
}