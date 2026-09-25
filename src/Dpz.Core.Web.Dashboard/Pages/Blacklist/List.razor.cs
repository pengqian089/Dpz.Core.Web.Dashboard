using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Blacklist;

public partial class List(IBlacklistService blacklistService, IAppDialogService dialogService)
{
    private const int PageSize = 24;
    private const int IpPreviewCount = 8;
    private const int UaPreviewCount = 2;
    private const int IpMatchLimit = 20;
    private const int UaMatchLimit = 8;
    private const int PathExpandThreshold = 60;

    private readonly List<BlacklistRecord> _blacklist = [];
    private readonly List<BlockedIpInfoModel> _blockedIps = [];
    private readonly BlockIpRequestModel _blockForm = new();
    private readonly HashSet<string> _expandedFields = [];
    private readonly Dictionary<string, string> _haystack = new();
    private List<BlacklistRecord> _filteredBlacklist = [];
    private List<BlockedIpInfoModel> _filteredBlockedIps = [];
    private List<BlacklistRecord> _pageBlacklist = [];
    private List<BlockedIpInfoModel> _pageBlockedIps = [];
    private SecurityView _view = SecurityView.Blacklist;
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isBlockFormOpen;
    private int _pageIndex = 1;
    private string _query = "";
    private string _queryTrimmed = "";
    private string _queryLower = "";

    private string Query
    {
        get => _query;
        set
        {
            if (_query == value)
            {
                return;
            }

            _query = value;
            _queryTrimmed = value.Trim();
            _queryLower = _queryTrimmed.ToLowerInvariant();
            _expandedFields.Clear();
            _pageIndex = 1;
            ApplyFilter();
        }
    }

    private bool IsSearching => _queryLower.Length > 0;

    private int TotalCount =>
        _view == SecurityView.Blacklist ? _filteredBlacklist.Count : _filteredBlockedIps.Count;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        try
        {
            if (_view == SecurityView.Blacklist)
            {
                _blacklist.Clear();
                _blacklist.AddRange(await blacklistService.GetBlacklistAsync());
                BuildSearchIndex();
            }
            else
            {
                _blockedIps.Clear();
                _blockedIps.AddRange(await blacklistService.GetBlockedIpsAsync());
            }
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载安全记录失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            ApplyFilter();
            _isLoading = false;
        }
    }

    private void BuildSearchIndex()
    {
        _haystack.Clear();
        foreach (var item in _blacklist)
        {
            var builder = new StringBuilder(item.RequestPath.Length + 64);
            builder.Append(item.RequestMethod).Append('\n').Append(item.RequestPath);
            foreach (var ip in item.IpAddresses)
            {
                builder.Append('\n').Append(ip);
            }
            foreach (var ua in item.UserAgents)
            {
                builder.Append('\n').Append(ua);
            }

            _haystack[item.Id] = builder.ToString().ToLowerInvariant();
        }
    }

    private async Task SwitchViewAsync(SecurityView view)
    {
        if (_view == view)
        {
            return;
        }

        _view = view;
        _isBlockFormOpen = false;
        _pageIndex = 1;
        await LoadAsync();
    }

    private Task ReloadAsync() => LoadAsync();

    private void ToggleBlockForm()
    {
        _isBlockFormOpen = !_isBlockFormOpen;
    }

    private async Task BlockIpAsync()
    {
        _isSaving = true;
        try
        {
            await blacklistService.BlockIpAsync(_blockForm);
            dialogService.Toast("IP 已封禁", ToastType.Success);
            _blockForm.Ip = string.Empty;
            _blockForm.Minutes = 30;
            _isBlockFormOpen = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"封禁失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteBlacklistAsync(BlacklistRecord item)
    {
        var confirmed = await dialogService.ConfirmAsync(
            $"确定删除 {item.RequestMethod} {item.RequestPath} 的黑名单记录吗？",
            "删除黑名单记录"
        );
        if (!confirmed)
        {
            return;
        }

        await blacklistService.DeleteBlacklistAsync(item.Id);
        dialogService.Toast("黑名单记录已删除", ToastType.Success);
        await LoadAsync();
    }

    private async Task UnblockIpAsync(BlockedIpInfoModel item)
    {
        var confirmed = await dialogService.ConfirmAsync($"确定解封 IP {item.Ip} 吗？", "解封 IP");
        if (!confirmed)
        {
            return;
        }

        await blacklistService.UnblockIpAsync(item.Ip);
        dialogService.Toast("IP 已解封", ToastType.Success);
        await LoadAsync();
    }

    private void ClearQuery()
    {
        Query = string.Empty;
    }

    private void ApplyFilter()
    {
        if (_queryLower.Length == 0)
        {
            _filteredBlacklist = [.. _blacklist];
            _filteredBlockedIps = [.. _blockedIps];
        }
        else
        {
            _filteredBlacklist = _blacklist
                .Where(item =>
                    _haystack.TryGetValue(item.Id, out var haystack)
                    && haystack.Contains(_queryLower, StringComparison.Ordinal)
                )
                .ToList();
            _filteredBlockedIps = _blockedIps
                .Where(item => item.Ip.Contains(_queryTrimmed, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (_pageIndex > TotalPages)
        {
            _pageIndex = TotalPages;
        }

        if (_pageIndex < 1)
        {
            _pageIndex = 1;
        }

        PageItems();
    }

    private void PageItems()
    {
        if (_view == SecurityView.Blacklist)
        {
            _pageBlacklist = _filteredBlacklist
                .Skip((_pageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            _pageBlockedIps.Clear();
        }
        else
        {
            _pageBlockedIps = _filteredBlockedIps
                .Skip((_pageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            _pageBlacklist.Clear();
        }
    }

    private Task OnPageChangedAsync(int page)
    {
        if (page == _pageIndex || page < 1 || page > TotalPages)
        {
            return Task.CompletedTask;
        }

        _pageIndex = page;
        PageItems();
        return Task.CompletedTask;
    }

    private bool IsFieldExpanded(string id, string field) =>
        _expandedFields.Contains(FieldKey(id, field));

    private bool IsPathExpanded(BlacklistRecord item) =>
        IsSearching || IsFieldExpanded(item.Id, "path");

    private void ToggleField(string id, string field)
    {
        var key = FieldKey(id, field);
        if (!_expandedFields.Remove(key))
        {
            _expandedFields.Add(key);
        }
    }

    private SubItemView BuildIps(BlacklistRecord item)
    {
        if (IsSearching)
        {
            var matches = new List<string>();
            foreach (var ip in item.IpAddresses)
            {
                if (ip.Contains(_queryTrimmed, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(ip);
                }
            }

            if (matches.Count > 0)
            {
                var visible =
                    matches.Count > IpMatchLimit ? matches.GetRange(0, IpMatchLimit) : matches;
                return new SubItemView(visible, matches.Count - visible.Count, false, false);
            }

            var preview =
                item.IpAddresses.Length > IpPreviewCount
                    ? item.IpAddresses[..IpPreviewCount]
                    : item.IpAddresses;
            return new SubItemView(preview, 0, false, false);
        }

        var expanded = IsFieldExpanded(item.Id, "ips");
        var shown =
            expanded || item.IpAddresses.Length <= IpPreviewCount
                ? item.IpAddresses
                : item.IpAddresses[..IpPreviewCount];
        return new SubItemView(shown, 0, item.IpAddresses.Length > IpPreviewCount, expanded);
    }

    private SubItemView BuildUserAgents(BlacklistRecord item)
    {
        if (IsSearching)
        {
            var matches = new List<string>();
            foreach (var ua in item.UserAgents)
            {
                if (ua.Contains(_queryTrimmed, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(ua);
                }
            }

            if (matches.Count > 0)
            {
                var visible =
                    matches.Count > UaMatchLimit ? matches.GetRange(0, UaMatchLimit) : matches;
                return new SubItemView(visible, matches.Count - visible.Count, false, false);
            }

            var preview =
                item.UserAgents.Length > UaPreviewCount
                    ? item.UserAgents[..UaPreviewCount]
                    : item.UserAgents;
            return new SubItemView(preview, 0, false, false);
        }

        var expanded = IsFieldExpanded(item.Id, "uas");
        var shown =
            expanded || item.UserAgents.Length <= UaPreviewCount
                ? item.UserAgents
                : item.UserAgents[..UaPreviewCount];
        return new SubItemView(shown, 0, item.UserAgents.Length > UaPreviewCount, expanded);
    }

    private RenderFragment Highlight(string? text) =>
        builder =>
        {
            var query = _queryTrimmed;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (query.Length == 0)
            {
                builder.AddContent(0, text);
                return;
            }

            var sequence = 0;
            var start = 0;
            while (true)
            {
                var index = text.IndexOf(query, start, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    break;
                }

                if (index > start)
                {
                    builder.AddContent(sequence++, text[start..index]);
                }

                builder.OpenElement(sequence++, "mark");
                builder.AddContent(sequence++, text.Substring(index, query.Length));
                builder.CloseElement();
                start = index + query.Length;
            }

            if (start < text.Length)
            {
                builder.AddContent(sequence, text[start..]);
            }
        };

    private static string FieldKey(string id, string field) => $"{id}:{field}";

    private static string FormatCount(int count, string suffix) => $"{count} {suffix}";

    private static string FormatTime(DateTime value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    private readonly record struct SubItemView(
        IReadOnlyList<string> Visible,
        int HiddenMatches,
        bool ShowToggle,
        bool IsExpanded
    );

    private enum SecurityView
    {
        Blacklist,
        BlockedIps,
    }
}
