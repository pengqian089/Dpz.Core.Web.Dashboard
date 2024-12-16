using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Service.Impl;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Comment;

public partial class List
{
    [Parameter] [SupplyParameterFromQuery] public string Node { get; set; }

    [Parameter] [SupplyParameterFromQuery] public string Relation { get; set; }

    [Inject] private ICommentService CommentService { get; set; }

    [Inject] private IDialogService DialogService { get; set; }
    
    [Inject]private IJSRuntime JsRuntime { get; set; }

    private MudTable<CommentModel> _table;

    private const int PageSize = 12;

    private int _pageIndex = 1;

    private CommentNode? _nodeType = null;

    private IDictionary<string, string> _secondItems = new Dictionary<string, string>();

    private bool _isLoading = true;

    private bool _isInit = true;

    private bool _isReload = false;

    #region temp

    private CommentNode? _tempNode = null;

    private string _tempRelation = null;
    private MudSelect<string> _secondSelect;

    #endregion

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsRuntime.InvokeVoidAsync("Prism.highlightAll");
        await base.OnAfterRenderAsync(firstRender);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        Console.WriteLine(Relation);
        if (Enum.TryParse(Node, out CommentNode node))
        {
            _nodeType = node;
            _secondItems = _nodeType switch
            {
                CommentNode.Article => await CommentService.GetArticleRelationAsync(),
                CommentNode.Code => await CommentService.CodeRelationAsync(),
                CommentNode.Other => await CommentService.OtherRelationAsync(),
                _ => new Dictionary<string, string>()
            };
        }
        else
        {
            _nodeType = null;
        }

        if (_table != null)
        {
            if (_nodeType == _tempNode && Relation == _tempRelation)
            {
            }
            else
            {
                _tempNode = _nodeType;
                _tempRelation = Relation;
                _isReload = true;
                await _table.ReloadServerData();
            }
        }

        await base.OnParametersSetAsync();
    }

    private async Task<TableData<CommentModel>> LoadAsync(TableState state)
    {
        _isLoading = true;
        if (_isInit)
        {
            await Task.Delay(500);
            _isInit = false;
        }

        _pageIndex = _isReload ? 1 : state.Page + 1;
        var list = await CommentService.GetPageAsync(_nodeType, Relation, _pageIndex, PageSize);
        _isReload = false;
        _isLoading = false;
        return new TableData<CommentModel>()
        {
            TotalItems = list.TotalItemCount,
            Items = list
        };
    }

    private async Task DeleteAsync(string id)
    {
        var result = await DialogService.ShowMessageBox(
            "提示",
            "删除后不能恢复，确定删除？",
            yesText: "删除!", cancelText: "取消");
        if (result == true)
        {
            await CommentService.ClearAsync(id);
            await _table.ReloadServerData();
        }
    }


    private void Search()
    {
        _isReload = true;
        _table.ReloadServerData();
    }

    private async Task OnNodeChange(string value)
    {
        await _secondSelect.Clear();
        if (Enum.TryParse(value, out CommentNode node))
        {
            Node = value;
            _nodeType = node;
            _secondItems = _nodeType switch
            {
                CommentNode.Article => await CommentService.GetArticleRelationAsync(),
                CommentNode.Code => await CommentService.CodeRelationAsync(),
                CommentNode.Other => await CommentService.OtherRelationAsync(),
                _ => new Dictionary<string, string>()
            };
        }
        else
        {
            _nodeType = null;
            Node = null;
            _secondItems = new Dictionary<string, string>();
        }
    }
}