using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.Article;

public partial class Edit(
    IArticleService articleService,
    IAppDialogService dialogService,
    NavigationManager navigation
)
{
    [Parameter]
    public string Id { get; set; } = "";

    private List<string> _tags = [];
    private ArticleEditRequest _article = new() { Id = "" };
    private bool _isLoading = true;
    private bool _isPublishing;
    private MarkdownEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        _tags = await articleService.GetTagsAsync();

        _isLoading = true;
        var article = await articleService.GetArticleAsync(Id);
        if (article == null)
        {
            dialogService.Toast("文章不存在", ToastType.Error);
            navigation.NavigateTo("/article");
            return;
        }
        _article = new ArticleEditRequest
        {
            Id = article.Id,
            Introduction = article.Introduction,
            Markdown = article.Markdown,
            Tags = article.Tags,
            Title = article.Title,
        };
        _isLoading = false;
        await base.OnInitializedAsync();
    }

    private void HandleNewTagAdded(string tag)
    {
        dialogService.Toast($"标签 '{tag}' 已添加", ToastType.Success);
    }

    private async Task PublishArticleAsync(EditContext context)
    {
        if (string.IsNullOrEmpty(_article.Id))
        {
            dialogService.Toast("缺少参数", ToastType.Error);
            return;
        }

        _article.Markdown = _editor == null ? null : await _editor.GetValueAsync();

        if (string.IsNullOrWhiteSpace(_article.Introduction))
        {
            dialogService.Toast("请输入文章简介", ToastType.Warning);
            return;
        }

        if (string.IsNullOrEmpty(_article.Markdown))
        {
            dialogService.Toast("请输入文章内容", ToastType.Warning);
            return;
        }

        _isPublishing = true;
        StateHasChanged();

        try
        {
            await articleService.EditAsync(_article);
            dialogService.Toast("文章修改成功", ToastType.Success);
            navigation.NavigateTo("/article");
        }
        catch (Exception ex)
        {
            _isPublishing = false;
            Console.WriteLine($"修改文章失败: {ex.Message}");
            dialogService.Toast("修改失败,请重试", ToastType.Error);
        }
    }
}
