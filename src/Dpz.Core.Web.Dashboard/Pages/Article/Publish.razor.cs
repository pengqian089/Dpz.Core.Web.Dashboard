using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Article;

public partial class Publish(
    IArticleService articleService,
    IAppDialogService dialogService,
    NavigationManager navigation
)
{
    private List<string> _tags = [];
    private string _addTag = "";
    private readonly ArticlePublishRequest _article = new();
    private bool _isPublishing;
    private MarkdownEditor _editor = null!;

    protected override async Task OnInitializedAsync()
    {
        _tags = await articleService.GetTagsAsync();
        await base.OnInitializedAsync();
    }

    private void ToggleTag(string tag)
    {
        var tags = _article.Tags.ToList();
        if (tags.Contains(tag))
        {
            tags.Remove(tag);
        }
        else
        {
            tags.Add(tag);
        }
        _article.Tags = tags;
    }

    private void AddNewTag()
    {
        if (string.IsNullOrWhiteSpace(_addTag))
        {
            return;
        }

        var trimmedTag = _addTag.Trim();
        if (_tags.Contains(trimmedTag))
        {
            dialogService.Toast("该标签已存在");
            _addTag = "";
            return;
        }

        _tags.Add(trimmedTag);
        ToggleTag(trimmedTag);
        _addTag = "";
        dialogService.Toast($"标签 '{trimmedTag}' 已添加", ToastType.Success);
    }

    private void HandleAddTagKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            AddNewTag();
        }
    }

    private async Task PublishArticleAsync(EditContext context)
    {
        _article.Markdown = await _editor.GetValueAsync();

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
            await _editor.DisposeAsync();
            navigation.NavigateTo("/article");
        }
        catch (Exception ex)
        {
            _isPublishing = false;
            Console.WriteLine($"发布文章失败: {ex.Message}");
            dialogService.Toast("发布失败,请重试", ToastType.Error);
        }
    }

    private void HandleTagsChange(ChangeEventArgs e)
    {
        if (e.Value is not string[] selectedTags)
        {
            return;
        }

        _article.Tags = selectedTags.ToList();
    }
}
