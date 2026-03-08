using System.Net.Http;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Dpz.Core.Web.Dashboard.Shared.Components;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.Mumble;

public partial class Edit(
    IMumbleService mumbleService,
    IAppDialogService dialogService,
    NavigationManager navigation)
    : ComponentBase
{
    [Parameter]
    public string Id { get; set; } = "";

    private readonly object _editModel = new();
    private bool _isLoading = true;
    private bool _isPublishing;
    private MumbleModel? _model;
    private MarkdownEditor? _editor;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            dialogService.Toast("缺少参数", ToastType.Error);
            navigation.NavigateTo("/mumble");
            return;
        }

        _isLoading = true;
        try
        {
            _model = await mumbleService.GetMumbleAsync(Id);
        }
        catch
        {
            dialogService.Toast("加载数据失败", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }

        await base.OnInitializedAsync();
    }

    private async Task PostDataAsync(EditContext context)
    {
        if (_model == null || string.IsNullOrWhiteSpace(_model.Id))
        {
            dialogService.Toast("缺少参数", ToastType.Error);
            return;
        }

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

        _isPublishing = true;
        StateHasChanged();

        try
        {
            await mumbleService.EditAsync(_model.Id, markdown);
            dialogService.Toast("保存成功", ToastType.Success);
            navigation.NavigateTo("/mumble");
        }
        catch
        {
            dialogService.Toast("保存失败，请重试", ToastType.Error);
            _isPublishing = false;
            StateHasChanged();
        }
    }
}
