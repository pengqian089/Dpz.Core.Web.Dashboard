using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.DynamicPage
{
    public partial class List
    {
        
        [Inject] private IDynamicPageService DynamicPageService { get; set; }

        [Inject] private IDialogService DialogService { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        #region query parameter

        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _id = "";

        #endregion

        private MudTable<DynamicPageModel> _table;

        #region temp

        private string _tempId = "";

        #endregion


        protected override async Task OnInitializedAsync()
        {
            _tempId = _id;
            await base.OnInitializedAsync();
        }

        private async Task<TableData<DynamicPageModel>> LoadDataAsync(TableState state)
        {
            if (_id == _tempId)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempId = _id;
            }

            var list = await DynamicPageService.GetPageAsync(_id, _pageIndex, PageSize);
            return new TableData<DynamicPageModel>()
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
                await DynamicPageService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }
    }
}