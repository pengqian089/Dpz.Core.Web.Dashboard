using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Blacklist;

public partial class List(IBlacklistService blacklistService, IAppDialogService dialogService)
{
    private const int IpPreviewCount = 8;
    private const int UaPreviewCount = 2;
    private const int PathExpandThreshold = 60;

    private readonly List<BlacklistRecord> _blacklist = [];
    private readonly List<BlockedIpInfoModel> _blockedIps = [];
    private readonly BlockIpRequestModel _blockForm = new();
    private readonly HashSet<string> _expandedFields = [];
    private List<BlacklistRecord> _filteredBlacklist = [];
    private List<BlockedIpInfoModel> _filteredBlockedIps = [];
    private SecurityView _view = SecurityView.Blacklist;
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isBlockFormOpen;
    private string _query = "";

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
            _expandedFields.Clear();
            ApplyFilter();
        }
    }

    private string NormalizedQuery => _query.Trim();

    private int MatchCount =>
        _view == SecurityView.Blacklist ? _filteredBlacklist.Count : _filteredBlockedIps.Count;

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

    private async Task SwitchViewAsync(SecurityView view)
    {
        if (_view == view)
        {
            return;
        }

        _view = view;
        _isBlockFormOpen = false;
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
        _filteredBlacklist =
            NormalizedQuery.Length == 0 ? [.. _blacklist] : _blacklist.Where(MatchesQuery).ToList();
        _filteredBlockedIps =
            NormalizedQuery.Length == 0
                ? [.. _blockedIps]
                : _blockedIps.Where(item => ContainsQuery(item.Ip)).ToList();
    }

    private bool MatchesQuery(BlacklistRecord item)
    {
        return ContainsQuery(item.RequestPath)
            || ContainsQuery(item.RequestMethod)
            || item.IpAddresses.Any(ContainsQuery)
            || item.UserAgents.Any(ContainsQuery);
    }

    private bool ContainsQuery(string? text)
    {
        return NormalizedQuery.Length > 0
            && !string.IsNullOrEmpty(text)
            && text.Contains(NormalizedQuery, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSearching => NormalizedQuery.Length > 0;

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

    private IReadOnlyList<string> VisibleIps(BlacklistRecord item) =>
        IsSearching || IsFieldExpanded(item.Id, "ips")
            ? item.IpAddresses
            : item.IpAddresses.Take(IpPreviewCount).ToArray();

    private IReadOnlyList<string> VisibleUserAgents(BlacklistRecord item) =>
        IsSearching || IsFieldExpanded(item.Id, "uas")
            ? item.UserAgents
            : item.UserAgents.Take(UaPreviewCount).ToArray();

    private RenderFragment Highlight(string? text) =>
        builder =>
        {
            var query = NormalizedQuery;
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

    private enum SecurityView
    {
        Blacklist,
        BlockedIps,
    }
}
