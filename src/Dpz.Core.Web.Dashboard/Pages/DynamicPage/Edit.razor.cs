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

public partial class Edit(
    IDynamicPageService dynamicPageService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    [Parameter]
    public string Id { get; set; } = "";

    private string _name = "";
    private readonly StringWriter _htmlContent = new();
    private readonly object _t = new();
    private bool _isPublishing;
    private bool _isLoading = true;
    private DynamicPageModel _model = new();
    private HtmlEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var model = await dynamicPageService.GetDynamicPageAsync(Id);
            if (model == null)
            {
                dialogService.Toast("未找到该页面", ToastType.Warning);
                return;
            }
            _model = model;
            _name = _model.Id ?? Id;
            var htmlContext = BrowsingContext.New(Configuration.Default);
            var html = await htmlContext.OpenAsync(x => x.Content(_model.Content ?? ""));
            html.ToHtml(_htmlContent, new PrettyMarkupFormatter());
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task PostDataAsync(EditContext context)
    {
        if (_editor == null)
        {
            dialogService.Toast("编辑器未初始化", ToastType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.Id))
        {
            dialogService.Toast("参数错误", ToastType.Warning);
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
            await dynamicPageService.EditDynamicPage(
                new SaveDynamicRequest
                {
                    HtmlContent = new HtmlContent { Content = content, Name = _model.Id },
                }
            );
            await _editor.DisposeAsync();
            dialogService.Toast("保存成功", ToastType.Success);
            navigation.NavigateTo("/dynamic");
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
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
