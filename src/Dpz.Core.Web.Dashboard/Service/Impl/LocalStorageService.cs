using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
    : ILocalStorageService
{
    public async Task<T?> GetItemAsync<T>(string key)
    {
        string? json;
        try
        {
            json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read '{Key}' from localStorage.", key);
            return default;
        }

        if (json == null)
        {
            return default;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)json;
        }

        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        string itemValue;
        if (value is null)
        {
            return;
        }
        if (value is string strValue)
        {
            itemValue = strValue;
        }
        else if (value is int intValue)
        {
            itemValue = intValue.ToString();
        }
        else
        {
            itemValue = JsonSerializer.Serialize(value);
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, itemValue);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write '{Key}' to localStorage.", key);
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove '{Key}' from localStorage.", key);
        }
    }
}
