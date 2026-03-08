using System;
using System.Threading.Tasks;
using BlazorMonaco.Editor;
using Dpz.Core.Web.Dashboard.Helper;
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

    private StandaloneCodeEditor? _editor;

    public async Task<string> GetValueAsync()
    {
        if (_editor == null)
        {
            return "";
        }
        return await _editor.GetValue();
    }

    public async ValueTask DisposeAsync()
    {
        if (_editor != null)
        {
            await _editor.DisposeEditor();
        }
    }

    public async Task InsertValueAsync(string value)
    {
        if (_editor == null)
        {
            await dialogService.AlertAsync("请等待编辑器加载完成");
            return;
        }
        var selection = await _editor.GetSelection();
        var distance = selection.EndColumn - selection.StartColumn;
        selection.CopyTo(out var endSelection);
        if (endSelection == null)
        {
            await dialogService.AlertAsync("未获取到选中区域");
            return;
        }
        endSelection.PositionColumn += value.Length - distance;
        endSelection.EndColumn += value.Length - distance;
        await _editor.ExecuteEdits(
            Guid.NewGuid().ToString(),
            [
                new IdentifiedSingleEditOperation
                {
                    ForceMoveMarkers = true,
                    Range = selection,
                    Text = value,
                },
            ],
            [endSelection]
        );
    }
}
