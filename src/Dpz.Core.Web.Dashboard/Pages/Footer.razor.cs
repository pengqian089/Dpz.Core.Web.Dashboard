using System;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Component;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Pages;

public partial class Footer
{

    [Inject]private ICommunityService CommunityService { get; set; }
    

    private readonly object _t = new();
    
    private bool _isLoading;

    private string _content = "";

    private HtmlEditor _editor;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _content = await CommunityService.GetFooterAsync();
        _isLoading = false;
        await base.OnInitializedAsync();
    }

    private async Task SaveAsync(EditContext context)
    {
        _isLoading = true;
        var content = await _editor.GetValueAsync();
        await CommunityService.SaveFooterAsync(content);
        await _editor.DisposeAsync();
        _content = await CommunityService.GetFooterAsync();
        _isLoading = false;
        StateHasChanged();
    }
}