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

public partial class Publish(
    IArticleService articleService,
    IAppDialogService dialogService,
    NavigationManager navigation
)
{
    private List<string> _tags = [];
    private readonly ArticlePublishRequest _article = new();
    private bool _isPublishing;
    private MarkdownEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        _tags = await articleService.GetTagsAsync();
        await base.OnInitializedAsync();
    }

    private void HandleNewTagAdded(string tag)
    {
        dialogService.Toast($"标签 '{tag}' 已添加", ToastType.Success);
    }

    private async Task PublishArticleAsync(EditContext context)
    {
        _article.Markdown = _editor == null ? null : await _editor.GetValueAsync();

        if (string.IsNullOrEmpty(_article.Title))
        {
            dialogService.Toast("请输入标题", ToastType.Warning);
            return;
        }
        if (string.IsNullOrEmpty(_article.Introduction))
        {
            dialogService.Toast("请输入简介", ToastType.Warning);
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
            if (await articleService.ExistsAsync(_article.Title))
            {
                _isPublishing = false;
                dialogService.Toast("该标题已经存在", ToastType.Warning);
                return;
            }

            await articleService.PublishAsync(_article);
            dialogService.Toast("文章发布成功", ToastType.Success);
            navigation.NavigateTo("/article");
        }
        catch (Exception ex)
        {
            _isPublishing = false;
            Console.WriteLine($"发布文章失败: {ex.Message}");
            dialogService.Toast("发布失败,请重试", ToastType.Error);
        }
    }
}
