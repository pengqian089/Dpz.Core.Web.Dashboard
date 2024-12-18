using System;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using MudBlazor;

#nullable enable

namespace Dpz.Core.Web.Dashboard.Pages.Video;

public partial class Screenshot
{
    [Parameter]
    public required string Id { get; set; }
    
    [CascadingParameter]
    private MudDialogInstance? MudDialog { get; set; }
    
    [Inject]
    private IVideoService? VideoService { get; set; }
    
    [Inject]
    private ISnackbar? Snackbar { get; set; }

    private double _seconds = 1;

    private bool _isLoading = true;

    private VideoMetaDataModel? _model;

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            try
            {
                _model = await VideoService?.GetVideoMetadataAsync(Id)!;
            }
            catch (Exception e)
            {
                ShowError(e.ToString());
            }
            _isLoading = false;
        }
        await base.OnParametersSetAsync();
    }

    private void ShowError(string message)
    {
        Snackbar?.Add(message, Severity.Error, option =>
        {
            option.MaximumOpacity = (int)TimeSpan.FromMinutes(5).TotalSeconds;
        });
    }

    private void Cancel()
    {
        MudDialog?.Cancel();
    }

    private async Task SaveAsync()
    {
        _isLoading = true;
        try
        {
            await VideoService?.SetVideoScreenshotAsync(Id, _seconds)!;
        }
        catch (Exception e)
        {
            ShowError(e.ToString());
        }
        _isLoading = false;
        MudDialog?.Close(DialogResult.Ok(true));
    }
}
