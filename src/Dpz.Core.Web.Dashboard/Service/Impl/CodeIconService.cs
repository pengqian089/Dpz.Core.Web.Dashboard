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

    private static readonly IReadOnlyDictionary<string, string> FallbackAliases = new Dictionary<
        string,
        string
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["slnx"] = "visualstudio",
    };

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(navigationManager.BaseUri),
    };

    private Task<IReadOnlyDictionary<string, string>>? _iconListTask;
    private Task<IReadOnlyList<FileIconRule>>? _fileIconsTask;
    private Task<IReadOnlyList<FolderIconRule>>? _folderIconsTask;
    private Task<LanguageMap>? _languageMapTask;
    private readonly object _cacheLock = new();

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
        var folderIcons = await GetFolderIconsAsync(cancellationToken);

        var folderIcon =
            folderIcons.FirstOrDefault(x => x.FolderNames.Contains(fileName))
            ?? folderIcons.FirstOrDefault(x => x.FolderNames.Contains(lowerFileName));

        return folderIcon?.Name ?? FolderIcon;
    }

    private async Task<string> MatchFileIconNameAsync(
        string fileName,
        string lowerFileName,
        CancellationToken cancellationToken
    )
    {
        var fileIcons = await GetFileIconsAsync(cancellationToken);
        var languageMap = await GetLanguageMapAsync(cancellationToken);

        var fileIcon =
            fileIcons.FirstOrDefault(x => x.FileNames.Contains(fileName))
            ?? fileIcons.FirstOrDefault(x => x.FileNames.Contains(lowerFileName));
        if (fileIcon != null)
        {
            return fileIcon.Name;
        }

        var fileExtensions = GetFileExtensions(fileName, lowerFileName);
        foreach (var extension in fileExtensions)
        {
            var extensionIcon = fileIcons.FirstOrDefault(x => x.FileExtensions.Contains(extension));
            if (extensionIcon != null)
            {
                return extensionIcon.Name;
            }
        }

        var languageMapIcon = MatchLanguageMapIcon(
            languageMap,
            fileName,
            lowerFileName,
            fileExtensions
        );
        if (languageMapIcon != null)
        {
            return languageMapIcon;
        }

        foreach (var extension in fileExtensions)
        {
            if (FallbackAliases.TryGetValue(extension, out var aliasIcon))
            {
                return aliasIcon;
            }
        }

        return FileIcon;
    }

    private static string? MatchLanguageMapIcon(
        LanguageMap languageMap,
        string fileName,
        string lowerFileName,
        IEnumerable<string> fileExtensions
    )
    {
        if (languageMap.FileNames.TryGetValue(fileName, out var fileNameIcon))
        {
            return fileNameIcon;
        }

        if (languageMap.FileNames.TryGetValue(lowerFileName, out var lowerFileNameIcon))
        {
            return lowerFileNameIcon;
        }

        foreach (var extension in fileExtensions)
        {
            if (languageMap.FileExtensions.TryGetValue(extension, out var extensionIcon))
            {
                return extensionIcon;
            }
        }

        return null;
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
        lock (_cacheLock)
        {
            _iconListTask ??= LoadIconListAsync(cancellationToken);
        }

        return await _iconListTask;
    }

    private async Task<IReadOnlyList<FileIconRule>> GetFileIconsAsync(
        CancellationToken cancellationToken
    )
    {
        lock (_cacheLock)
        {
            _fileIconsTask ??= LoadFileIconsAsync(cancellationToken);
        }

        return await _fileIconsTask;
    }

    private async Task<IReadOnlyList<FolderIconRule>> GetFolderIconsAsync(
        CancellationToken cancellationToken
    )
    {
        lock (_cacheLock)
        {
            _folderIconsTask ??= LoadFolderIconsAsync(cancellationToken);
        }

        return await _folderIconsTask;
    }

    private async Task<LanguageMap> GetLanguageMapAsync(CancellationToken cancellationToken)
    {
        lock (_cacheLock)
        {
            _languageMapTask ??= LoadLanguageMapAsync(cancellationToken);
        }

        return await _languageMapTask;
    }

    private async Task<IReadOnlyDictionary<string, string>> LoadIconListAsync(
        CancellationToken cancellationToken
    )
    {
        return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
            "data/icon-list.json",
            cancellationToken
        ) ?? new Dictionary<string, string>();
    }

    private async Task<IReadOnlyList<FileIconRule>> LoadFileIconsAsync(
        CancellationToken cancellationToken
    )
    {
        return await _httpClient.GetFromJsonAsync<List<FileIconRule>>(
            "data/fileIcons.json",
            cancellationToken
        ) ?? [];
    }

    private async Task<IReadOnlyList<FolderIconRule>> LoadFolderIconsAsync(
        CancellationToken cancellationToken
    )
    {
        return await _httpClient.GetFromJsonAsync<List<FolderIconRule>>(
            "data/folderIcons.json",
            cancellationToken
        ) ?? [];
    }

    private async Task<LanguageMap> LoadLanguageMapAsync(CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<LanguageMap>(
            "data/language-map.json",
            cancellationToken
        ) ?? new LanguageMap();
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

    private sealed class FileIconRule
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("fileNames")]
        public List<string> FileNames { get; set; } = [];

        [JsonPropertyName("fileExtensions")]
        public List<string> FileExtensions { get; set; } = [];
    }

    private sealed class FolderIconRule
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("folderNames")]
        public List<string> FolderNames { get; set; } = [];
    }
}
