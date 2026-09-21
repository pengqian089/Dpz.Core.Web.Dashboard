using System;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared;

public partial class MainLayout(
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigation,
    IJSRuntime jsRuntime,
    IAppDialogService dialogService
) : LayoutComponentBase, IAsyncDisposable
{
    private bool _drawerOpen;
    private IJSObjectReference? _jsModule;
    private NavigationManager Navigation => navigation;

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    protected override async Task OnInitializedAsync()
    {
        await CheckPermissionAsync();
        authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        navigation.LocationChanged += OnLocationChanged;
    }

    private async Task CheckPermissionAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (
            !Enum.TryParse(
                authState.User.FindFirst("Permissions")?.Value ?? "",
                out Permissions permissions
            )
            || (permissions & Permissions.System) != Permissions.System
        )
        {
            navigation.NavigateTo("/no-permission");
        }
    }

    private void HandleAuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
    {
        _ = CheckPermissionAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Shared/MainLayout.razor.js"
                );
                if (_jsModule != null)
                {
                    await _jsModule.InvokeVoidAsync("initDropdowns");
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowAlertAsync(
                    new AppDialogOptions
                    {
                        Title = "错误",
                        Message = $"界面初始化失败：{ex.Message}",
                    }
                );
            }
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (_drawerOpen)
        {
            _drawerOpen = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        navigation.LocationChanged -= OnLocationChanged;
        authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChanged;

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose");
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // ignore
            }
        }
    }

    private Task BeginLogout()
    {
        var returnUrl = Uri.EscapeDataString(navigation.Uri);
        navigation.NavigateToLogout($"authentication/logout?returnUrl={returnUrl}");
        return Task.CompletedTask;
    }
}
