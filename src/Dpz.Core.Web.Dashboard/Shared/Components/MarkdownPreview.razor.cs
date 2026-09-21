using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Service;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class MarkdownPreview(
    IJSRuntime jsRuntime,
    IAssetManifestService assetManifestService,
    ILogger<MarkdownPreview> logger
) : ComponentBase, IAsyncDisposable
{
    private ElementReference _contentRef;
    private IJSObjectReference? _module;
    private string _htmlContent = "";

    [Parameter]
    [EditorRequired]
    public required string Markdown { get; set; }

    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// 是否将仓库内源码链接（以 src 开头）重写为代码浏览页地址，
    /// 并为外部链接添加新标签页打开属性。
    /// </summary>
    [Parameter]
    public bool EnableCodeTreeLinks { get; set; }

    private static readonly HashSet<string> ExternalSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "http",
        "https",
        "ftp",
        "mailto",
        "tel",
        "data",
    };

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
        if (EnableCodeTreeLinks)
        {
            RewriteCodeTreeLinks(document);
        }

        _htmlContent = document.ToHtml(pipeline);

        await base.OnParametersSetAsync();
    }

    private static void RewriteCodeTreeLinks(MarkdownDocument document)
    {
        foreach (var link in document.Descendants<LinkInline>())
        {
            if (link.IsImage)
            {
                continue;
            }

            var url = link.Url?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                continue;
            }

            if (IsExternalUrl(url))
            {
                AddNewTabAttributes(link);
                continue;
            }

            var normalized = url.Replace('\\', '/');
            while (normalized.StartsWith("./", StringComparison.Ordinal))
            {
                normalized = normalized[2..];
            }

            normalized = normalized.TrimStart('/');
            var suffixStart = normalized.IndexOfAny(['#', '?']);
            var path = suffixStart >= 0 ? normalized[..suffixStart] : normalized;
            if (
                !string.Equals(path, "src", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("src/", StringComparison.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            var suffix = suffixStart >= 0 ? normalized[suffixStart..] : string.Empty;
            link.Url = $"/code/tree?path={HttpUtility.UrlEncode(path)}{suffix}";
        }

        foreach (var link in document.Descendants<AutolinkInline>())
        {
            link.GetAttributes().AddPropertyIfNotExist("target", "_blank");
            link.GetAttributes().AddPropertyIfNotExist("rel", "noopener noreferrer");
        }
    }

    private static bool IsExternalUrl(string url)
    {
        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && ExternalSchemes.Contains(uri.Scheme);
    }

    private static void AddNewTabAttributes(LinkInline link)
    {
        link.GetAttributes().AddPropertyIfNotExist("target", "_blank");
        link.GetAttributes().AddPropertyIfNotExist("rel", "noopener noreferrer");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var modulePath = await assetManifestService.GetAssetPathAsync(
                "src/markdown-preview.ts"
            );
            try
            {
                _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to import markdown preview module.");
            }
        }

        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("highlightCodeBlocks", _contentRef);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to highlight code blocks.");
            }
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
