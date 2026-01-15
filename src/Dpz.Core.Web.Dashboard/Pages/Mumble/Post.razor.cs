using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.Mumble;

public partial class Post(
    IMumbleService mumbleService,
    IAppDialogService dialogService,
    NavigationManager navigation)
    : ComponentBase
{
    private readonly object _model = new();
    private bool _isPublishing;
    private MarkdownEditor? _editor;

    private async Task PostDataAsync(EditContext context)
    {
        if (_editor == null)
        {
            dialogService.Toast("编辑器未初始化", ToastType.Error);
            return;
        }

        var markdown = await _editor.GetValueAsync();
        if (string.IsNullOrWhiteSpace(markdown))
        {
            dialogService.Toast("请输入内容", ToastType.Warning);
            return;
        }

        var content = Markdown.ToHtml(markdown);
        if (string.IsNullOrWhiteSpace(content))
        {
            dialogService.Toast("内容解析失败", ToastType.Error);
            return;
        }

        _isPublishing = true;
        StateHasChanged();

        try
        {
            await mumbleService.CreateAsync(markdown, content);
            dialogService.Toast("发布成功", ToastType.Success);
            navigation.NavigateTo("/mumble");
        }
        catch
        {
            dialogService.Toast("发布失败，请重试", ToastType.Error);
            _isPublishing = false;
            StateHasChanged();
        }
    }
}
