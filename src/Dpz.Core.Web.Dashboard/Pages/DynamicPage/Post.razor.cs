using System;
using System.IO;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.DynamicPage;

public partial class Post(
    IDynamicPageService dynamicPageService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    private string _name = "";
    private readonly StringWriter _htmlContent = new();
    private readonly object _t = new();
    private bool _isPublishing;
    private HtmlEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        var htmlContext = BrowsingContext.New(Configuration.Default);
        var html = await htmlContext.OpenAsync(x => x.Content("<!DOCTYPE html>"));

        html.DocumentElement.SetAttribute("lang", "zh-Hans");
        html.Title = "新建动态页";
        var h1 = html.CreateElement("h1");
        h1.TextContent = "新建动态页";
        html.Body?.Append(h1);
        html.ToHtml(_htmlContent, new PrettyMarkupFormatter());
    }

    private async Task PostDataAsync(EditContext context)
    {
        if (_editor == null)
        {
            dialogService.Toast("编辑器未初始化", ToastType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(_name))
        {
            dialogService.Toast("请输入页面名称", ToastType.Warning);
            return;
        }

        var content = await _editor.GetValueAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            dialogService.Toast("请输入内容", ToastType.Warning);
            return;
        }

        _isPublishing = true;
        StateHasChanged();
        try
        {
            if (await dynamicPageService.ExistsAsync(_name))
            {
                dialogService.Toast("该页面名称已存在", ToastType.Warning);
                return;
            }

            await dynamicPageService.CreateDynamicPage(
                new SaveDynamicRequest
                {
                    HtmlContent = new HtmlContent { Content = content, Name = _name },
                }
            );
            await _editor.DisposeAsync();
            dialogService.Toast("发布成功", ToastType.Success);
            navigation.NavigateTo("/dynamic");
        }
        catch (Exception ex)
        {
            dialogService.Toast($"发布失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isPublishing = false;
            StateHasChanged();
        }
    }

    private void BackToList()
    {
        navigation.NavigateTo("/dynamic");
    }

    private static string BuildPreviewUrl(string name)
    {
        var baseUrl = Program.WebHost.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(name))
        {
            return $"{baseUrl}/act/<name>";
        }

        return $"{baseUrl}/act/{name}";
    }
}
