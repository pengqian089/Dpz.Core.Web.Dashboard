using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Music
{
    public partial class List
    {
        [Inject] private IMusicService MusicService { get; set; }

        [Inject] private IDialogService DialogService { get; set; }

        #region query parameter
        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _title = "";

        #endregion


        private List<string> _tags = new();

        private MudTable<MusicModel> _table;

        #region temp

        private string _tempTitle = "";

        #endregion


        protected override async Task OnInitializedAsync()
        {
            _tempTitle = _title;
            await base.OnInitializedAsync();
        }

        private async Task<TableData<MusicModel>> LoadDataAsync(TableState state)
        {
            if (_title == _tempTitle)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempTitle = _title;
            }
            var list = await MusicService.GetPageAsync(_title ,_pageIndex, PageSize);
            return new TableData<MusicModel>()
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
                await MusicService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }
    }
}