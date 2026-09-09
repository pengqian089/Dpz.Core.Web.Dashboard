using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;

namespace Dpz.Core.Web.Dashboard.Pages.Blacklist;

public partial class List(IBlacklistService blacklistService, IAppDialogService dialogService)
{
    private readonly List<BlacklistRecord> _blacklist = [];
    private readonly List<BlockedIpInfoModel> _blockedIps = [];
    private readonly BlockIpRequestModel _blockForm = new();
    private SecurityView _view = SecurityView.Blacklist;
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isBlockFormOpen;

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

    private static string FormatCount(int count, string suffix) => $"{count} {suffix}";

    private static string FormatUserAgents(IReadOnlyList<string> userAgents)
    {
        return string.Join(" | ", userAgents);
    }

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
