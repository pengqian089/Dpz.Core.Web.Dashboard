using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class Editor(IAppDialogService dialogService) : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public string Markdown { get; set; } = "";

    [Parameter]
    [EditorRequired]
    public required string ElementId { get; set; }

    private CodeEditor? _editor;

    public async Task<string> GetValueAsync()
    {
        if (_editor == null)
        {
            return "";
        }

        return await _editor.GetValueAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_editor != null)
        {
            await _editor.DisposeAsync();
        }
    }

    public async Task InsertValueAsync(string value)
    {
        if (_editor == null)
        {
            await dialogService.AlertAsync("请等待编辑器加载完成");
            return;
        }

        await _editor.InsertValueAsync(value);
    }
}
