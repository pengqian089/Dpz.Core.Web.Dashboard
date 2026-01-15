using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class HtmlEditor : ComponentBase, IAsyncDisposable
{
    private readonly string _elementId = Guid.NewGuid().ToString("N");
    private StandaloneCodeEditor? _editor;

    [Parameter]
    public string Html { get; set; } = "";

    public async Task<string> GetValueAsync()
    {
        if (_editor == null)
        {
            return "";
        }

        return await _editor.GetValue();
    }

    public async Task InsertValueAsync(string value)
    {
        if (_editor == null)
        {
            return;
        }

        var selection = await _editor.GetSelection();
        var distance = selection.EndColumn - selection.StartColumn;
        
        var endSelection = new Selection();
        endSelection.SelectionStartLineNumber = selection.SelectionStartLineNumber;
        endSelection.SelectionStartColumn = selection.SelectionStartColumn;
        endSelection.PositionLineNumber = selection.PositionLineNumber;
        endSelection.PositionColumn = selection.PositionColumn + value.Length - distance;
        endSelection.StartLineNumber = selection.StartLineNumber;
        endSelection.StartColumn = selection.StartColumn;
        endSelection.EndLineNumber = selection.EndLineNumber;
        endSelection.EndColumn = selection.EndColumn + value.Length - distance;
        
        await _editor.ExecuteEdits(
            Guid.NewGuid().ToString(),
            new List<IdentifiedSingleEditOperation>
            {
                new()
                {
                    ForceMoveMarkers = true,
                    Range = selection,
                    Text = value
                }
            },
            new List<Selection> { endSelection });
    }

    public async ValueTask DisposeAsync()
    {
        if (_editor != null)
        {
            await _editor.DisposeEditor();
        }
    }
}
