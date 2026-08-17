using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using TinyPinyin;

namespace Dpz.Core.Web.Dashboard.Shared;

public partial class NavMenu(ILocalStorageService localStorageService, ILogger<NavMenu> logger)
{
    private const string CollapsedGroupsStorageKey = "nav-menu-collapsed-groups";

    private static readonly IReadOnlyList<NavGroup> Groups =
    [
        new(
            "menu",
            "菜单",
            [
                new("", NavLinkMatch.All, "fas fa-gauge", "首页"),
                new("article", NavLinkMatch.Prefix, "fas fa-file-lines", "文章管理"),
                new("picture", NavLinkMatch.Prefix, "fas fa-images", "相册管理"),
            ]
        ),
        new(
            "audio",
            "音频管理",
            [
                new("music", NavLinkMatch.Prefix, "fas fa-music", "音乐管理"),
                new("audio", NavLinkMatch.Prefix, "fas fa-microphone", "录音管理"),
            ]
        ),
        new(
            "content",
            "内容管理",
            [
                new("mumble", NavLinkMatch.Prefix, "fas fa-comment-dots", "碎碎念管理"),
                new("timeline", NavLinkMatch.Prefix, "fas fa-stream", "时间轴管理"),
                new("danmaku", NavLinkMatch.Prefix, "fas fa-comments", "弹幕管理"),
                new("dynamic", NavLinkMatch.Prefix, "fas fa-pager", "动态页管理"),
            ]
        ),
        new(
            "comments",
            "评论管理",
            [
                new("comment", NavLinkMatch.All, "fas fa-comments", "所有评论"),
                new("comment/Article", NavLinkMatch.All, "fas fa-file-alt", "文章评论"),
                new("comment/Friends", NavLinkMatch.All, "fas fa-link", "友链评论"),
                new("comment/Code", NavLinkMatch.All, "fas fa-code", "源码评论"),
                new("comment/Mumble", NavLinkMatch.All, "fas fa-comment-dots", "碎碎念评论"),
                new("comment/Other", NavLinkMatch.All, "fas fa-comment", "其他评论"),
            ]
        ),
        new(
            "system",
            "系统管理",
            [
                new("video", NavLinkMatch.Prefix, "fas fa-video", "视频管理"),
                new("account", NavLinkMatch.Prefix, "fas fa-users", "用户管理"),
                new("friends", NavLinkMatch.Prefix, "fas fa-link", "友情链接"),
                new("code/tree", NavLinkMatch.Prefix, "fas fa-code-branch", "源码管理"),
                new("footer", NavLinkMatch.Prefix, "fas fa-info-circle", "页脚内容"),
                new("system-notification", NavLinkMatch.Prefix, "fas fa-bullhorn", "系统通知"),
                new("message-outbox", NavLinkMatch.Prefix, "fas fa-route", "消息队列"),
                new("robots", NavLinkMatch.Prefix, "fas fa-robot", "Robots.txt"),
                new("seo", NavLinkMatch.Prefix, "fas fa-search", "SEO 管理"),
            ]
        ),
    ];

    private static readonly IReadOnlyList<NavGroup> SearchableGroups = Groups
        .Select(group =>
            group with
            {
                Items = group
                    .Items.Select(item =>
                        item with
                        {
                            SearchIndex = CreateSearchIndex(group.Title, item),
                        }
                    )
                    .ToArray(),
            }
        )
        .ToArray();

    private string _searchText = "";
    private readonly HashSet<string> _collapsedGroupKeys = new(StringComparer.Ordinal);

    private bool IsSearchActive => !string.IsNullOrWhiteSpace(_searchText);

    private IReadOnlyList<NavGroup> FilteredGroups
    {
        get
        {
            var keyword = Normalize(_searchText);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return SearchableGroups;
            }

            return SearchableGroups
                .Select(group =>
                    group with
                    {
                        Items = group
                            .Items.Where(item =>
                                item.SearchIndex.Contains(keyword, StringComparison.Ordinal)
                            )
                            .ToArray(),
                    }
                )
                .Where(group => group.Items.Count > 0)
                .ToArray();
        }
    }

    private void ClearSearch()
    {
        _searchText = "";
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var savedKeys = await localStorageService.GetItemAsync<string[]>(
                CollapsedGroupsStorageKey
            );
            var knownKeys = Groups.Select(group => group.Key).ToHashSet(StringComparer.Ordinal);
            _collapsedGroupKeys.UnionWith(
                savedKeys?.Where(key => key is not null && knownKeys.Contains(key))
                    ?? Enumerable.Empty<string>()
            );
        }
        catch (Exception ex) when (ex is JsonException or JSException)
        {
            logger.LogWarning(ex, "Failed to load navigation group collapse state");
        }
    }

    private bool IsGroupExpanded(NavGroup group)
    {
        return IsSearchActive || !_collapsedGroupKeys.Contains(group.Key);
    }

    private string GetGroupToggleLabel(NavGroup group, bool isExpanded)
    {
        if (IsSearchActive)
        {
            return $"{group.Title}（搜索中展开）";
        }

        return isExpanded ? $"收起{group.Title}" : $"展开{group.Title}";
    }

    private async Task ToggleGroupAsync(string groupKey)
    {
        if (IsSearchActive)
        {
            return;
        }

        if (!_collapsedGroupKeys.Add(groupKey))
        {
            _collapsedGroupKeys.Remove(groupKey);
        }

        try
        {
            await localStorageService.SetItemAsync(
                CollapsedGroupsStorageKey,
                _collapsedGroupKeys.OrderBy(key => key, StringComparer.Ordinal).ToArray()
            );
        }
        catch (JSException ex)
        {
            logger.LogWarning(ex, "Failed to save navigation group collapse state");
        }
    }

    private static string CreateSearchIndex(string groupTitle, NavItem item)
    {
        var text = $"{groupTitle} {item.Label} {item.Href}";
        var pinyin = PinyinHelper.GetPinyin(text, "");
        var pinyinWithSpace = PinyinHelper.GetPinyin(text, " ");
        var firstPinyin = PinyinHelper.GetPinyinInitials(text, "");
        return Normalize($"{text} {pinyin} {pinyinWithSpace} {firstPinyin}");
    }

    private static string Normalize(string value)
    {
        return value.Trim().Replace(" ", "", StringComparison.Ordinal).ToLowerInvariant();
    }

    private sealed record NavGroup(string Key, string Title, IReadOnlyList<NavItem> Items);

    private sealed record NavItem(string Href, NavLinkMatch Match, string Icon, string Label)
    {
        public string SearchIndex { get; init; } = "";
    }
}
