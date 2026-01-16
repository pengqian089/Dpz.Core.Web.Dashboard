using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages.Article;

public partial class List(
    IArticleService articleService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private const int PageSize = 10;
    private string _tag = "";
    private string _title = "";
    private List<string> _tags = [];
    private List<ArticleModel>? _articles;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _tags = await articleService.GetTagsAsync();
        ReadQueryParameters();
        await LoadArticlesAsync();
        await base.OnInitializedAsync();
    }

    private void ReadQueryParameters()
    {
        var uri = new Uri(navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (int.TryParse(query["page"], out var page) && page > 0)
        {
            _pageIndex = page;
        }

        _tag = query["tag"] ?? "";
        _title = query["title"] ?? "";
    }

    private void UpdateUrl()
    {
        var baseUri = navigation.ToAbsoluteUri("/article").GetLeftPart(UriPartial.Path);
        var queryParams = new List<string>();

        if (_pageIndex > 1)
        {
            queryParams.Add($"page={_pageIndex}");
        }

        if (!string.IsNullOrWhiteSpace(_tag))
        {
            queryParams.Add($"tag={Uri.EscapeDataString(_tag)}");
        }

        if (!string.IsNullOrWhiteSpace(_title))
        {
            queryParams.Add($"title={Uri.EscapeDataString(_title)}");
        }

        var url = queryParams.Count > 0 ? $"{baseUri}?{string.Join("&", queryParams)}" : baseUri;

        navigation.NavigateTo(url, false);
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        UpdateUrl();
        await LoadArticlesAsync();
    }

    private async Task LoadArticlesAsync()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var result = await articleService.GetPageAsync(_pageIndex, PageSize, _tag, _title);
            _articles = result.ToList();
            _totalCount = result.TotalItemCount;
            _totalPages = result.TotalPageCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载文章列表失败: {ex.Message}");
            _articles = [];
            _totalCount = 0;
            _totalPages = 0;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task Search()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadArticlesAsync();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Search();
        }
    }

    private void PublishArticle()
    {
        navigation.NavigateTo("/article/publish");
    }

    private void EditArticle(string id)
    {
        navigation.NavigateTo($"/article/edit/{id}");
    }

    private async Task DeleteAsync(string id)
    {
        var confirmed = await dialogService.ConfirmAsync(
            "删除后不能恢复，确定删除吗？",
            "确认删除"
        );

        if (confirmed)
        {
            try
            {
                await articleService.DeleteAsync(id);
                dialogService.Toast("文章删除成功", Models.Dialog.ToastType.Success);
                await LoadArticlesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除文章失败: {ex.Message}");
                dialogService.Toast("删除失败，请重试", Models.Dialog.ToastType.Error);
            }
        }
    }

    private static string HighlightText(string text, string highlight)
    {
        if (string.IsNullOrWhiteSpace(highlight))
        {
            return text;
        }

        return text;
    }
}
