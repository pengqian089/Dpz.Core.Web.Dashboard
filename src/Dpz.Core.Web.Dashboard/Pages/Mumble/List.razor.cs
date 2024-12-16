using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Mumble
{
    public partial class List
    {
        [Inject] private IMumbleService MumbleService { get; set; }

        [Inject] private IDialogService DialogService { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        #region query parameter

        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _content = "";

        #endregion


        private List<string> _tags = new();

        private MudTable<MumbleModel> _table;

        #region temp

        private string _tempContent = "";

        #endregion


        protected override async Task OnInitializedAsync()
        {
            _tempContent = _content;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JsRuntime.InvokeVoidAsync("Prism.highlightAll");
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task<TableData<MumbleModel>> LoadDataAsync(TableState state)
        {
            if (_content == _tempContent)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempContent = _content;
                _pageIndex = 1;
            }

            var list = await MumbleService.GetPageAsync(_content, _pageIndex, PageSize);
            return new TableData<MumbleModel>()
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
                await MumbleService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }

        private void Preview(string html)
        {
            var parameters = new DialogParameters {["Html"] = html};
            DialogService.Show<Preview>("",parameters, new DialogOptions {CloseButton = true});
        }
    }
}