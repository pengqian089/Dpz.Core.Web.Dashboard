using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Danmaku
{
    public partial class List
    {
        [Inject]
        private IDanmakuService DanmakuService { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private IVideoService VideoService { get; set; }

        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _text = "";

        private string _group = "";

        private MudTable<DanmakuModel> _table;

        private string _tempText = "";

        private string _tempGroup = "";

        private List<string> _groups = new();

        private Dictionary<string, string> _groupDic = new();

        private HashSet<DanmakuModel> _selectedItems = new();

        protected override async Task OnInitializedAsync()
        {
            _tempText = _text;
            _tempGroup = _group;
            await base.OnInitializedAsync();
        }

        private async Task InitAsync()
        {
            if (_groups.Any() && _groupDic.Any())
                return;
            _groups = await DanmakuService.GetGroupsAsync();
            var videos = await VideoService.GetVideosAsync();
            _groups.AddRange(videos.Select(x => x.Id));
            _groups = _groups.GroupBy(x => x).Select(x => x.Key).ToList();
            _groupDic = _groups.ToDictionary(
                x => x,
                x =>
                {
                    var video = videos.FirstOrDefault(y => y.Id == x);
                    return !string.IsNullOrEmpty(video?.Name) ? video.Name : x;
                }
            );
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JsRuntime.InvokeVoidAsync("Prism.highlightAll");
            await base.OnAfterRenderAsync(firstRender);
        }

        private bool _isLoading = true;

        private async Task<TableData<DanmakuModel>> LoadDataAsync(TableState state)
        {
            _isLoading = true;
            await InitAsync();
            if (_text == _tempText && _group == _tempGroup)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempText = _text;
                _tempGroup = _group;
                _pageIndex = 1;
            }
            _selectedItems.Clear();
            var list = await DanmakuService.GetPageAsync(_text, _group, _pageIndex, PageSize);
            _isLoading = false;
            return new TableData<DanmakuModel>() { TotalItems = list.TotalItemCount, Items = list };
        }

        private void Search()
        {
            _table.ReloadServerData();
        }

        private async Task OnDeleteClick()
        {
            if (!_selectedItems.Any())
            {
                await DialogService.ShowMessageBox(
                    "提示",
                    "请至少选择一项数据！",
                    yesText: "确定",
                    cancelText: "取消"
                );
                return;
            }
            var result = await DialogService.ShowMessageBox(
                "提示",
                "删除后不能恢复，确定删除？",
                yesText: "删除!",
                cancelText: "取消"
            );
            if (result == true)
            {
                await DanmakuService.DeleteAsync(_selectedItems.Select(x => x.Id).ToArray());
                await _table.ReloadServerData();
            }
        }

        private async Task OnImportAcfun()
        {
            if (!_groupDic.Any())
                return;
            var parameters = new DialogParameters
            {
                ["Title"] = "导入AcFun弹幕",
                ["ExtensionName"] = ".json",
                ["Groups"] = _groupDic,
            };
            var dialog = await DialogService.ShowAsync<Import>(
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

        private async Task OnImportBilibili()
        {
            if (!_groupDic.Any())
                return;
            var parameters = new DialogParameters
            {
                ["Title"] = "导入bilibili弹幕",
                ["ExtensionName"] = ".xml",
                ["Groups"] = _groupDic,
            };
            var dialog = await DialogService.ShowAsync<Import>(
                "",
                parameters,
                new DialogOptions { CloseButton = true }
            );
            var result = await dialog.Result;
            if (result?.Canceled == false && bool.TryParse(result.Data?.ToString() ?? "", out var r) && r)
            {
                Search();
            }
        }

        private Task OnDanmakuSelected(HashSet<DanmakuModel> items)
        {
            _selectedItems = items;
            return Task.CompletedTask;
        }
    }
}
