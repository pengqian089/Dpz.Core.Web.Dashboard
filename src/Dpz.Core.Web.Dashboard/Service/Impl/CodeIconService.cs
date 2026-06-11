using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class CodeIconService(NavigationManager navigationManager) : ICodeIconService
{
    private const string CdnPrefix = "https://dpangzi.com/icons/";
    private const string FolderIcon = "folder";
    private const string FileIcon = "file";
    private const string GitFolderIcon = "folder-git";
    private const string SymlinkFolderIcon = "folder-symlink";

    private static readonly IReadOnlyDictionary<string, string> ExtensionAliases = new Dictionary<
        string,
        string
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["slnx"] = "visualstudio"
    };

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(navigationManager.BaseUri),
    };

    private IReadOnlyDictionary<string, string>? _iconList;
    private LanguageMap? _languageMap;

    public async Task<string> GetIconUrlAsync(
        string? name,
        bool isFolder,
        bool isSubmodule = false,
        bool isSymlink = false,
        CancellationToken cancellationToken = default
    )
    {
        var iconName = await MatchIconNameAsync(
            name,
            isFolder,
            isSubmodule,
            isSymlink,
            cancellationToken
        );
        var iconFileName = await GetIconFileNameAsync(iconName, isFolder, cancellationToken);

        return CdnPrefix + iconFileName;
    }

    private async Task<string> MatchIconNameAsync(
        string? name,
        bool isFolder,
        bool isSubmodule,
        bool isSymlink,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GetDefaultIconName(isFolder);
        }

        var fileName = NormalizeName(name);
        var lowerFileName = fileName.ToLowerInvariant();

        if (isSubmodule)
        {
            return GitFolderIcon;
        }

        if (isSymlink)
        {
            return SymlinkFolderIcon;
        }

        return isFolder
            ? await MatchFolderIconNameAsync(fileName, lowerFileName, cancellationToken)
            : await MatchFileIconNameAsync(fileName, lowerFileName, cancellationToken);
    }

    private async Task<string> MatchFolderIconNameAsync(
        string fileName,
        string lowerFileName,
        CancellationToken cancellationToken
    )
    {
        var iconList = await GetIconListAsync(cancellationToken);
        var iconName = $"folder-{fileName}";
        if (iconList.ContainsKey(iconName))
        {
            return iconName;
        }

        iconName = $"folder-{lowerFileName}";
        return iconList.ContainsKey(iconName) ? iconName : FolderIcon;
    }

    private async Task<string> MatchFileIconNameAsync(
        string fileName,
        string lowerFileName,
        CancellationToken cancellationToken
    )
    {
        var languageMap = await GetLanguageMapAsync(cancellationToken);
        var iconList = await GetIconListAsync(cancellationToken);

        if (languageMap.FileNames.TryGetValue(fileName, out var fileNameIcon))
        {
            return fileNameIcon;
        }

        if (languageMap.FileNames.TryGetValue(lowerFileName, out var lowerFileNameIcon))
        {
            return lowerFileNameIcon;
        }

        var fileExtensions = GetFileExtensions(fileName, lowerFileName);
        foreach (var extension in fileExtensions)
        {
            if (languageMap.FileExtensions.TryGetValue(extension, out var extensionIcon))
            {
                return extensionIcon;
            }

            if (iconList.ContainsKey(extension))
            {
                return extension;
            }

            if (
                ExtensionAliases.TryGetValue(extension, out var aliasIcon)
                && iconList.ContainsKey(aliasIcon)
            )
            {
                return aliasIcon;
            }
        }

        return FileIcon;
    }

    private async Task<string> GetIconFileNameAsync(
        string iconName,
        bool isFolder,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return GetDefaultIconFileName(isFolder);
        }

        var iconList = await GetIconListAsync(cancellationToken);
        return iconList.TryGetValue(iconName, out var iconFileName)
            ? iconFileName
            : GetDefaultIconFileName(isFolder);
    }

    private async Task<IReadOnlyDictionary<string, string>> GetIconListAsync(
        CancellationToken cancellationToken
    )
    {
        if (_iconList != null)
        {
            return _iconList;
        }

        _iconList =
            await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
                "data/icon-list.json",
                cancellationToken
            ) ?? new Dictionary<string, string>();

        return _iconList;
    }

    private async Task<LanguageMap> GetLanguageMapAsync(CancellationToken cancellationToken)
    {
        if (_languageMap != null)
        {
            return _languageMap;
        }

        _languageMap =
            await _httpClient.GetFromJsonAsync<LanguageMap>(
                "data/language-map.json",
                cancellationToken
            ) ?? new LanguageMap();

        return _languageMap;
    }

    private static string NormalizeName(string name)
    {
        var lastPath =
            name.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? name;
        return string.Join(
            ' ',
            lastPath.Split(default(char[]?), StringSplitOptions.RemoveEmptyEntries)
        );
    }

    private static List<string> GetFileExtensions(string fileName, string lowerFileName)
    {
        if (fileName.Length > 255)
        {
            return [];
        }

        var extensions = new List<string>();
        for (var i = 0; i < fileName.Length; i++)
        {
            if (fileName[i] == '.' && i + 1 < fileName.Length)
            {
                extensions.Add(lowerFileName[(i + 1)..]);
            }
        }

        return extensions;
    }

    private static string GetDefaultIconName(bool isFolder) => isFolder ? FolderIcon : FileIcon;

    private static string GetDefaultIconFileName(bool isFolder) =>
        isFolder ? "folder.svg" : "file.svg";

    private sealed class LanguageMap
    {
        [JsonPropertyName("fileExtensions")]
        public Dictionary<string, string> FileExtensions { get; set; } = [];

        [JsonPropertyName("fileNames")]
        public Dictionary<string, string> FileNames { get; set; } = [];
    }
}
