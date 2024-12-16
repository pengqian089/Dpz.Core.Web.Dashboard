using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Article
{
    public partial class List
    {
        [Inject]private IArticleService ArticleService { get; set; }

        [Inject]private NavigationManager Navigation { get; set; }

        [Inject] private IDialogService DialogService { get; set; }

        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _tag = "";

        private string _title = "";

        private List<string> _tags = new();

        private MudTable<ArticleModel> _table;

        private string _tempTag = "";

        private string _tempTitle = "";

        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            _tempTag = _tag;
            _tempTitle = _title;
            _tags = await ArticleService.GetTagsAsync();
            await base.OnInitializedAsync();
        }

        private async Task<TableData<ArticleModel>> LoadArticleAsync(TableState state)
        {
            _isLoading = true;
            if (_tag == _tempTag && _title == _tempTitle)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempTag = _tag;
                _tempTitle = _title;
                _pageIndex = 1;
            }
            var list = await ArticleService.GetPageAsync(_pageIndex, PageSize, _tag, _title);
            _isLoading = false;
            return new TableData<ArticleModel>()
            {
                TotalItems = list.TotalItemCount, 
                Items = list
            };
        }

        private void Search()
        {
            _table.ReloadServerData();
        }

        private void PublishArticle()
        {
            Navigation.NavigateTo("/article/publish");
        }

        private void EditArticle(string id)
        {
            Navigation.NavigateTo($"/article/edit/{id}");
        }

        private async Task DeleteAsync(string id)
        {
            var result = await DialogService.ShowMessageBox(
                "提示",
                "删除后不能恢复，确定删除？",
                yesText: "删除!", cancelText: "取消");
            if (result == true)
            {
                await ArticleService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }
    }
}