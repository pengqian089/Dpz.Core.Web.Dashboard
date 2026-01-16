using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
{
    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

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

        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, itemValue);
    }

    public async Task RemoveItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
