using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class MarkdownEditor(
    IConfiguration configuration,
    IJSRuntime jsRuntime,
    ILocalStorageService localStorageService
) : ComponentBase
{
    [Parameter]
    public required string Markdown { get; set; }

    [Parameter]
    public required string UploadAction { get; set; }

    [Parameter]
    public int? Height { get; set; }

    [Parameter]
    public string HeightUnit { get; set; } = "px";

    private string HeightStyle => Height == null ? "" : $"height:{Height}{HeightUnit}";

    private readonly string _editorId = Guid.NewGuid().ToString("N");

    private string? _baseAddress;

    private static int _number;

    protected override async Task OnInitializedAsync()
    {
        var editorTheme = await localStorageService.GetItemAsync<string>("cherry-theme");
        if (string.IsNullOrWhiteSpace(editorTheme))
        {
            await localStorageService.SetItemAsync("cherry-theme", "dark");
        }
        _baseAddress = configuration.GetSection("BaseAddress").Get<string>();
#if DEBUG
        Console.WriteLine(
            "markdown editor init: {0},ElementId:{1},Markdown:{2}",
            _number++,
            _editorId,
            Markdown
        );
#endif
        await Task.FromResult(base.OnInitializedAsync());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await jsRuntime.InvokeVoidAsync("createNewEditor", _editorId, Markdown);
        }
    }

    public async Task<string> GetValueAsync()
    {
        return await jsRuntime.InvokeAsync<string>("getMarkdown");
    }
}
