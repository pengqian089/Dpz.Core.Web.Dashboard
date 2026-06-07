using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages.Danmaku;

public partial class DanmakuImportDialog(
    IDanmakuService danmakuService,
    IAppDialogService dialogService
)
{
    [Parameter]
    public IReadOnlyDictionary<string, string> Groups { get; set; } =
        new Dictionary<string, string>();

    [Parameter]
    public string Extension { get; set; } = "";

    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    private readonly string InputId = $"importDanmakuFile-{Guid.NewGuid():N}";
    private string _importGroup = "";
    private IBrowserFile? _selectedFile;
    private bool _isImporting;

    private void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
    }

    private void ClearSelectedFile()
    {
        _selectedFile = null;
    }

    private Task CancelAsync()
    {
        CloseDialog?.Invoke(false);
        return Task.CompletedTask;
    }

    private async Task ImportDanmakuAsync()
    {
        if (_selectedFile == null)
        {
            await dialogService.ShowAlertAsync(
                new AppDialogOptions { Message = "请选择要导入的弹幕文件！" }
            );
            return;
        }

        if (string.IsNullOrEmpty(_importGroup))
        {
            await dialogService.ShowAlertAsync(
                new AppDialogOptions { Message = "请选择弹幕分组！" }
            );
            return;
        }

        _isImporting = true;
        StateHasChanged();

        try
        {
            using var content = new MultipartFormDataContent();
            var groupContent = new StringContent(_importGroup);
            content.Add(content: groupContent, name: "\"Group\"");

            var fileContent = new StreamContent(_selectedFile.OpenReadStream(AppTools.MaxFileSize));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(_selectedFile.ContentType);
            content.Add(content: fileContent, name: "\"File\"", fileName: _selectedFile.Name);

            if (Extension == ".json")
            {
                await danmakuService.ImportAcfunAsync(content);
            }
            else if (Extension == ".xml")
            {
                await danmakuService.ImportBilibiliAsync(content);
            }

            dialogService.ShowToast(
                new AppToastOptions
                {
                    Message = "导入成功！",
                    Level = AppFeedbackLevel.Success,
                }
            );
            CloseDialog?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.ShowToast(
                new AppToastOptions
                {
                    Message = $"导入失败: {ex.Message}",
                    Level = AppFeedbackLevel.Danger,
                    Duration = 5000,
                }
            );
        }
        finally
        {
            _isImporting = false;
            StateHasChanged();
        }
    }
}
