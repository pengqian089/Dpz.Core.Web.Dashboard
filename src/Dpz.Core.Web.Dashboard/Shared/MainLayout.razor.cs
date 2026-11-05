using System;
using System.Text.Json;
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
    ILocalStorageService localStorageService,
    IAppDialogService dialogService
) : LayoutComponentBase, IAsyncDisposable
{
    private const string SidebarCollapsedStorageKey = "app-sidebar-collapsed";

    // 移动端抽屉：导航后默认收起，不做持久化。
    private bool _drawerOpen;

    // 桌面端折叠：写入 localStorage，刷新或再次导航后保持。
    private bool _sidebarCollapsed;
    private IJSObjectReference? _jsModule;
    private NavigationManager Navigation => navigation;

    protected override async Task OnInitializedAsync()
    {
        _sidebarCollapsed = await LoadSidebarCollapsedAsync();
        await CheckPermissionAsync();
        authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        navigation.LocationChanged += OnLocationChanged;
    }

    private async Task<bool> LoadSidebarCollapsedAsync()
    {
        try
        {
            return await localStorageService.GetItemAsync<bool>(SidebarCollapsedStorageKey);
        }
        catch (Exception ex) when (ex is JsonException or JSException)
        {
            // 本地存储中的数据不可读时回退为展开状态。
            return false;
        }
    }

    private async Task ToggleSidebarAsync()
    {
        if (await IsMobileAsync())
        {
            _drawerOpen = !_drawerOpen;
            return;
        }

        _sidebarCollapsed = !_sidebarCollapsed;

        try
        {
            await localStorageService.SetItemAsync(SidebarCollapsedStorageKey, _sidebarCollapsed);
        }
        catch (JSException)
        {
            // 本地存储不可用时仅影响记忆能力，界面折叠仍然生效。
        }
    }

    private void CloseDrawer()
    {
        _drawerOpen = false;
    }

    private async Task<bool> IsMobileAsync()
    {
        var module = await EnsureJsModuleAsync();
        if (module is null)
        {
            return false;
        }

        try
        {
            return await module.InvokeAsync<bool>("isMobileViewport");
        }
        catch (JSException)
        {
            return false;
        }
    }

    private async Task<IJSObjectReference?> EnsureJsModuleAsync()
    {
        if (_jsModule is not null)
        {
            return _jsModule;
        }

        try
        {
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Shared/MainLayout.razor.js"
            );
        }
        catch (JSException)
        {
            return null;
        }

        return _jsModule;
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
                var module = await EnsureJsModuleAsync();
                if (module is null)
                {
                    await ShowInitErrorAsync("无法加载 MainLayout 脚本模块。");
                    return;
                }

                await module.InvokeVoidAsync("initDropdowns");
            }
            catch (Exception ex)
            {
                await ShowInitErrorAsync($"界面初始化失败：{ex.Message}");
            }
        }
    }

    private Task ShowInitErrorAsync(string message)
    {
        return dialogService.ShowAlertAsync(
            new AppDialogOptions { Title = "错误", Message = message }
        );
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        // 移动端导航后收起抽屉；桌面端的折叠状态由 _sidebarCollapsed 保存，不受导航影响。
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
