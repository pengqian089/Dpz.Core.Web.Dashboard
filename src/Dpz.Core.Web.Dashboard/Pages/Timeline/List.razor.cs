using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Timeline
{
    public partial class List
    {
        [Inject]private ITimelineService TimelineService { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }
        
        [Inject] private IDialogService DialogService { get; set; }

        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _title = "";

        private MudTable<TimelineModel> _table;

        private string _tempTitle = "";

        protected override async Task OnInitializedAsync()
        {
            _tempTitle = _title;
            await base.OnInitializedAsync();
        }
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JsRuntime.InvokeVoidAsync("Prism.highlightAll");
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task<TableData<TimelineModel>> LoadDataAsync(TableState state)
        {
            if (_title == _tempTitle)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempTitle = _title;
                _pageIndex = 1;
            }
            var list = await TimelineService.GetPageAsync(_title,_pageIndex, PageSize);
            return new TableData<TimelineModel>()
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
                await TimelineService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }
    }
}