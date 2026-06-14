using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class AssetManifestService(NavigationManager navigationManager) : IAssetManifestService
{
    private readonly string _manifestRequestId = Guid.NewGuid().ToString("N");

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(navigationManager.BaseUri),
    };

    private IReadOnlyDictionary<string, ManifestEntry>? _manifest;

    public async Task<string> GetAssetPathAsync(string entryName)
    {
        var manifest = await GetManifestAsync();
        if (
            manifest.TryGetValue(entryName, out var entry) && !string.IsNullOrWhiteSpace(entry.File)
        )
        {
            return $"./assets/{entry.File}";
        }

        throw new InvalidOperationException($"Vite manifest entry '{entryName}' was not found.");
    }

    private async Task<IReadOnlyDictionary<string, ManifestEntry>> GetManifestAsync()
    {
        if (_manifest != null)
        {
            return _manifest;
        }

        _manifest =
            await _httpClient.GetFromJsonAsync<Dictionary<string, ManifestEntry>>(
                $"assets/.vite/manifest.json?v={_manifestRequestId}"
            ) ?? new Dictionary<string, ManifestEntry>();

        return _manifest;
    }

    private sealed record ManifestEntry([property: JsonPropertyName("file")] string? File);
}
