using System;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Dpz.Core.Web.Dashboard.Shared;

public partial class MainLayout(
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigation
) : LayoutComponentBase, IDisposable
{
    // Default closed for mobile first approach
    private bool _drawerOpen;

    // Expose Navigation for Razor usage
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

        // Listen to navigation events to close drawer on mobile
        navigation.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        // Close drawer on navigation (UX improvement for mobile)
        if (_drawerOpen)
        {
            _drawerOpen = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        navigation.LocationChanged -= OnLocationChanged;
    }

    private Task BeginLogout()
    {
        var returnUrl = Uri.EscapeDataString(navigation.Uri);
        navigation.NavigateToLogout($"authentication/logout?returnUrl={returnUrl}");
        return Task.CompletedTask;
    }
}
