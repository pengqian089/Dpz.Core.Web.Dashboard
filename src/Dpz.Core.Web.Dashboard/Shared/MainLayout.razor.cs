using System;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Shared;

public partial class MainLayout(
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigation,
    IJSRuntime jsRuntime
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
        navigation.LocationChanged += OnLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Shared/MainLayout.razor.js"
            );
            await _jsModule.InvokeVoidAsync("initDropdowns");
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
