using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Code;

public partial class NoteForm(ICodeService codeService, IAppDialogService dialogService)
{
    [Parameter]
    public CodeSaveModel Model { get; set; } = new CodeSaveModel { Name = "" };

    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    private string _note = "";
    private bool _isSubmitting;
    private bool _isInitialized;

    private string DisplayPath =>
        Model.Path is { Length: > 0 } ? string.Join("/", Model.Path) : "/";

    protected override void OnParametersSet()
    {
        if (_isInitialized)
        {
            return;
        }

        _note = Model.Note ?? "";
        _isInitialized = true;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            dialogService.Toast("参数错误", ToastType.Warning);
            return;
        }

        _isSubmitting = true;
        try
        {
            var payload = new CodeSaveModel
            {
                Name = Model.Name,
                Path = Model.Path,
                Note = string.IsNullOrWhiteSpace(_note) ? null : _note.Trim(),
            };

            await codeService.SaveNoteAsync(payload);
            dialogService.Toast("保存成功", ToastType.Success);
            CloseDialog?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel()
    {
        CloseDialog?.Invoke(null);
    }
}
