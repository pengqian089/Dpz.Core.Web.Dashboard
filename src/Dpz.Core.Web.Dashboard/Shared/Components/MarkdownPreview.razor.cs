using System;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class MarkdownPreview(IJSRuntime jsRuntime) : ComponentBase, IAsyncDisposable
{
    private ElementReference _contentRef;
    private IJSObjectReference? _module;
    private string _htmlContent = "";

    [Parameter]
    [EditorRequired]
    public required string Markdown { get; set; }

    [Parameter]
    public string? Style { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAutoLinks()
            .UsePipeTables()
            .UseTaskLists()
            .UseEmphasisExtras()
            .UseFooters()
            .UseCitations()
            .UseMathematics()
            .UseAutoIdentifiers()
            .Build();

        var document = Markdig.Markdown.Parse(Markdown, pipeline);
        _htmlContent = document.ToHtml(pipeline);

        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Shared/Components/MarkdownPreview.razor.js"
            );
        }

        if (_module != null)
        {
            await _module.InvokeVoidAsync("highlightCodeBlocks", _contentRef);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }
}
