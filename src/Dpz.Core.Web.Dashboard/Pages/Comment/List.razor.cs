using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Dpz.Core.Web.Dashboard.Pages.Comment;

public partial class List(
    ICommentService commentService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    [Parameter]
    public string? NodeParam { get; set; }

    private const int PageSize = 12;
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private bool _isLoading = true;
    private string _nodeValue = "";
    private CommentNode? _nodeType;
    private string _relation = "";
    private List<CommentRelationResponse> _relationItems = [];
    private List<CommentModel> _comments = [];

    protected override async Task OnParametersSetAsync()
    {
        ReadParameters();
        await LoadRelationsAsync();
        await LoadDataAsync();
    }

    private void ReadParameters()
    {
        var uri = new Uri(navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (int.TryParse(query["page"], out var page) && page > 0)
        {
            _pageIndex = page;
        }
        else
        {
            _pageIndex = 1;
        }

        _nodeValue = NodeParam ?? "";
        _relation = query["relation"] ?? "";

        if (Enum.TryParse(_nodeValue, out CommentNode node))
        {
            _nodeType = node;
        }
        else
        {
            _nodeType = null;
        }
    }

    private void UpdateUrl()
    {
        var basePath = string.IsNullOrWhiteSpace(_nodeValue)
            ? "/comment"
            : $"/comment/{_nodeValue}";

        var queryParams = new List<string>();

        if (_pageIndex > 1)
        {
            queryParams.Add($"page={_pageIndex}");
        }

        if (!string.IsNullOrWhiteSpace(_relation))
        {
            queryParams.Add($"relation={Uri.EscapeDataString(_relation)}");
        }

        var url = queryParams.Count > 0 ? $"{basePath}?{string.Join("&", queryParams)}" : basePath;
        navigation.NavigateTo(url, false);
    }

    private async Task LoadRelationsAsync()
    {
        if (_nodeType == null)
        {
            _relationItems = [];
            _relation = "";
            return;
        }

        _relationItems = _nodeType switch
        {
            CommentNode.Article => await commentService.GetArticleRelationAsync(),
            CommentNode.Code => await commentService.CodeRelationAsync(),
            CommentNode.Other => await commentService.OtherRelationAsync(),
            _ => [],
        };

        if (!_relationItems.Any(x => x.Id == _relation))
        {
            _relation = "";
        }
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        var result = await commentService.GetPageAsync(_nodeType, _relation, _pageIndex, PageSize);
        _comments = result.ToList();
        _totalCount = result.TotalItemCount;
        _totalPages = result.TotalPageCount;

        _isLoading = false;
        StateHasChanged();
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task Search()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task Reload()
    {
        await LoadDataAsync();
    }

    private async Task HandleNodeChanged()
    {
        if (Enum.TryParse(_nodeValue, out CommentNode node))
        {
            _nodeType = node;
        }
        else
        {
            _nodeType = null;
        }

        _relation = "";
        await LoadRelationsAsync();
        await Search();
    }

    private async Task HandleRelationChanged()
    {
        await Search();
    }

    private async Task ViewContent(CommentModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CommentText))
        {
            dialogService.Toast("当前内容为空", ToastType.Warning);
            return;
        }

        await dialogService.ShowComponentAsync(
            "评论内容",
            BuildCommentPreview(model.CommentText),
            "900px"
        );
    }

    private async Task DeleteAsync(string id)
    {
        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (confirmed)
        {
            await commentService.ClearAsync(id);
            dialogService.Toast("删除成功", ToastType.Success);
            await LoadDataAsync();
        }
    }

    private static RenderFragment BuildCommentPreview(string content)
    {
        return builder =>
        {
            builder.OpenComponent<MarkdownPreview>(0);
            builder.AddAttribute(1, "Markdown", content);
            builder.AddAttribute(2, "Style", "background: transparent; border: none;");
            builder.CloseComponent();
        };
    }

    private string GetNodeText(CommentNode node)
    {
        return node switch
        {
            CommentNode.Article => "文章",
            CommentNode.Code => "源码",
            CommentNode.Friends => "友链",
            CommentNode.Mumble => "碎碎念",
            CommentNode.Steam => "Steam",
            CommentNode.Video => "视频",
            CommentNode.Other => "其它",
            _ => "未知",
        };
    }

    private string GetRelationTitle(string relation)
    {
        var item = _relationItems.FirstOrDefault(x => x.Id == relation);
        return item?.Title ?? relation;
    }

    private static bool IsLongComment(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        return content.Length > 120 || content.Contains('\n');
    }

    private static string GetCommentPreview(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "";
        }

        var singleLine = content.Replace("\r", " ").Replace("\n", " ");
        return singleLine.Length <= 120 ? singleLine : $"{singleLine[..120]}...";
    }

    private static string GetAvatarUrl(CommentModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.EmailMd5))
        {
            return $"https://cravatar.cn/avatar/{model.EmailMd5}?d=wavatar";
        }

        if (!string.IsNullOrWhiteSpace(model.Avatar))
        {
            return model.Avatar;
        }

        return "https://cravatar.cn/avatar/?d=wavatar";
    }
}
