using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.Web.Dashboard.Service.Impl
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            ILocalStorageService localStorageService,
            NavigationManager navigationManager,
            HttpClient httpClient,
            ILogger<AuthenticationService> logger
        )
        {
            _localStorageService = localStorageService;
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _logger = logger;
        }

        public AppUser User { get; private set; }

        public async Task InitializeAsync()
        {
            try
            {
                User = await _localStorageService.GetItemAsync<AppUser>("Identity");
            }
            catch (Exception)
            {
                User = null;
            }
        }

        public async Task<(bool result, string message)> SignAsync(string account, string password, string pinCode)
        {
            var msg = "";
            try
            {
                var responseMessage =
                    await _httpClient.PostAsJsonAsync("/api/Community/Blazor/auth", new {account, password, pinCode});
                if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    msg = await responseMessage.Content.ReadAsStringAsync();
                    return (false, msg);
                }

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var error = await responseMessage.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    _logger.LogDebug("sign fail,error:{Exception}", error);
                    return (false, msg);
                }

                var user = await responseMessage.Content.ReadFromJsonAsync<AppUser>();
                if (user?.Account.Permissions.HasValue == true &&
                    user.Account.Permissions.Value.HasFlag(Permissions.System))
                {
                    await _localStorageService.SetItemAsync("Identity", user);
                    User = user;
                    return (true, msg);
                }

                return (false, msg);
            }
            catch (Exception e)
            {
                _logger.LogDebug("sign fail,exception:{Exception}", e);
                return (false, e.Message);
            }
        }

        public async Task SignOutAsync()
        {
            User = null;
            await _localStorageService.RemoveItemAsync("Identity");
            _navigationManager.NavigateTo("/login");
        }

        public async Task<bool> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                var responseMessage =
                    await _httpClient.PostAsJsonAsync("/api/Community/Blazor/refresh",
                        new {accessToken = token, refreshToken});
                if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    return false;
                }

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var error = await responseMessage.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    _logger.LogDebug("refresh token fail,error:{Exception}", error);
                    return false;
                }

                var user = await responseMessage.Content.ReadFromJsonAsync<AppUser>();
                await _localStorageService.SetItemAsync("Identity", user);
                User = user;
                return true;
            }
            catch (Exception e)
            {
                _logger.LogDebug("refresh token fail,exception:{Exception}", e);
                return false;
            }
        }
    }
}