using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Picture
{
    public partial class List
    {
        [Inject] private IPictureService PictureService { get; set; }

        [Inject] private NavigationManager Navigation { get; set; }

        [Inject] private IDialogService DialogService { get; set; }

        #region query parameter
        private int _pageIndex = 1;

        private const int PageSize = 12;

        private string _tag = "";

        private string _description = "";

        private int _pictureType = -1;

        #endregion


        private List<string> _tags = new();

        private MudTable<PictureResponseModel> _table;

        private readonly Func<int, string> _convert = x => Enum.ToObject(typeof(PictureType), x).ToString();

        #region temp
        private string _tempTag = "";

        private string _tempDescription = "";

        private int _tempPictureType = -1;
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _tempTag = _tag;
            _tempDescription = _description;
            _tempPictureType = _pictureType;

            _tags = await PictureService.GetTagsAsync();
            await base.OnInitializedAsync();
        }

        private async Task<TableData<PictureResponseModel>> LoadDataAsync(TableState state)
        {
            if (_tag == _tempTag && _description == _tempDescription && _pictureType == _tempPictureType)
            {
                _pageIndex = state.Page + 1;
            }
            else
            {
                _tempTag = _tag;
                _tempDescription = _description;
                _tempPictureType = _pictureType;
            }
            var list = await PictureService.GetPageAsync(_tag,_description,_pictureType,_pageIndex, PageSize);
            return new TableData<PictureResponseModel>()
            {
                TotalItems = list.TotalItemCount,
                Items = list
            };
        }

        private void Search()
        {
            _table.ReloadServerData();
        }

        private void PostPicture()
        {
            Navigation.NavigateTo("/picture/post");
        }

        private void EditPicture(string id)
        {
            Navigation.NavigateTo($"/picture/edit/{id}");
        }

        private async Task DeleteAsync(string id)
        {
            var result = await DialogService.ShowMessageBox(
                "提示",
                "删除后不能恢复，确定删除？",
                yesText: "删除!", cancelText: "取消");
            if (result == true)
            {
                await PictureService.DeleteAsync(id);
                await _table.ReloadServerData();
            }
        }
    }
}