using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class HtmlEditor : ComponentBase, IAsyncDisposable
{
    private readonly string _elementId = Guid.NewGuid().ToString("N");
    private CodeEditor? _editor;

    [Parameter]
    public string Html { get; set; } = "";

    public async Task<string> GetValueAsync()
    {
        if (_editor == null)
        {
            return "";
        }

        return await _editor.GetValueAsync();
    }

    public async Task InsertValueAsync(string value)
    {
        if (_editor == null)
        {
            return;
        }

        await _editor.InsertValueAsync(value);
    }

    public async ValueTask DisposeAsync()
    {
        if (_editor != null)
        {
            await _editor.DisposeAsync();
        }
    }
}
