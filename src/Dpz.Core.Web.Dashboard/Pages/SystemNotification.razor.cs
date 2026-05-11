using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages;

public partial class SystemNotification(
    ISystemNotificationService notificationService,
    IAppDialogService dialogService,
    NavigationManager navigation
)
{
    private const int MaxMessageLength = 500;
    private const int HistoryPageSize = 10;
    private readonly object _formState = new();
    private bool _isSending;
    private bool _isHistoryLoading = true;
    private string _message = "";
    private DateTime? _lastSentAt;
    private string _lastSentMessage = "";
    private int _historyPageIndex = 1;
    private int _historyTotalCount;
    private int _historyTotalPages;
    private List<SystemNotificationHistoryResponse> _histories = [];

    private int MessageLength => _message.Length;

    private string HubUrl => notificationService.HubUrl;

    private bool CanSend =>
        !_isSending && !string.IsNullOrWhiteSpace(_message) && MessageLength <= MaxMessageLength;

    private string CounterClass =>
        MessageLength > MaxMessageLength * 0.9 ? "system-notification__counter--warning" : "";

    private string PreviewMessage =>
        string.IsNullOrWhiteSpace(_message)
            ? "这里会实时显示通知内容，发送后前台用户将收到这条消息。"
            : _message.Trim();

    protected override async Task OnInitializedAsync()
    {
        ReadQueryParameters();
        await LoadHistoryAsync();
    }

    private void ReadQueryParameters()
    {
        var uri = new Uri(navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (int.TryParse(query["page"], out var page) && page > 0)
        {
            _historyPageIndex = page;
        }
    }

    private void UpdateUrl()
    {
        var baseUri = navigation.ToAbsoluteUri("/system-notification").GetLeftPart(UriPartial.Path);
        var url = _historyPageIndex > 1 ? $"{baseUri}?page={_historyPageIndex}" : baseUri;

        navigation.NavigateTo(url, false);
    }

    private static readonly NotificationTemplate[] Templates =
    [
        new(
            "维护公告",
            "fas fa-screwdriver-wrench",
            "系统将在今晚 23:30 进行短暂维护，期间部分功能可能不可用，请提前保存正在编辑的内容。"
        ),
        new(
            "服务恢复",
            "fas fa-circle-check",
            "系统维护已完成，所有服务已恢复正常。感谢您的耐心等待。"
        ),
        new("功能提醒", "fas fa-lightbulb", "新功能已上线，欢迎体验。如遇异常请刷新页面后重试。"),
    ];

    private void UseTemplate(string message)
    {
        _message = message;
    }

    private void ClearMessage()
    {
        _message = "";
    }

    private async Task LoadHistoryAsync()
    {
        _isHistoryLoading = true;
        StateHasChanged();

        try
        {
            var list = await notificationService.GetPageAsync(_historyPageIndex, HistoryPageSize);
            _histories = list.ToList();
            _historyTotalCount = list.TotalItemCount;
            _historyTotalPages = list.TotalPageCount;
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载通知记录失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isHistoryLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshHistoryAsync()
    {
        await LoadHistoryAsync();
    }

    private async Task HandleHistoryPageChanged(int page)
    {
        if (page < 1 || page > _historyTotalPages || page == _historyPageIndex)
        {
            return;
        }

        _historyPageIndex = page;
        UpdateUrl();
        await LoadHistoryAsync();
    }

    private async Task DeleteHistoryAsync(SystemNotificationHistoryResponse item)
    {
        if (string.IsNullOrWhiteSpace(item.DisplayId))
        {
            dialogService.Toast("记录 ID 为空，无法删除", ToastType.Error);
            return;
        }

        var confirmed = await dialogService.ConfirmAsync(
            "删除后不能恢复，确定删除这条通知记录？",
            "删除通知记录"
        );
        if (!confirmed)
        {
            return;
        }

        try
        {
            await notificationService.DeleteAsync(item.DisplayId);
            dialogService.Toast("通知记录已删除", ToastType.Success);

            if (_histories.Count == 1 && _historyPageIndex > 1)
            {
                _historyPageIndex--;
                UpdateUrl();
            }

            await LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"删除失败：{ex.Message}", ToastType.Error);
        }
    }

    private static string FormatTime(SystemNotificationHistoryResponse item)
    {
        return item.DisplayTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未知时间";
    }

    private async Task SendAsync(EditContext context)
    {
        var message = _message.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            dialogService.Toast("请输入通知内容", ToastType.Warning);
            return;
        }

        var confirmed = await dialogService.ConfirmAsync(
            "确认向前台所有在线用户发送这条系统通知吗？",
            "发送系统通知"
        );
        if (!confirmed)
        {
            return;
        }

        _isSending = true;
        StateHasChanged();

        try
        {
            await notificationService.SendAsync(message);
            _lastSentAt = DateTime.Now;
            _lastSentMessage = message;
            _message = "";
            dialogService.Toast("系统通知已发送", ToastType.Success);
            _historyPageIndex = 1;
            UpdateUrl();
            await LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"发送失败：{ex.Message}", ToastType.Error, 5000);
        }
        finally
        {
            _isSending = false;
            StateHasChanged();
        }
    }

    private sealed record NotificationTemplate(string Title, string Icon, string Message);
}
