using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.MessageOutbox;

public partial class List(
    IMessageOutboxService messageOutboxService,
    IAppDialogService dialogService,
    NavigationManager navigation
)
{
    private const int PageSize = 15;
    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private bool _isLoading = true;
    private string? _reconsumingId;
    private MessageOutboxRequest _request = new();
    private MessageOutboxFilterOptionsResponse _filterOptions = new();
    private List<MessageOutboxResponse> _items = [];

    private static readonly OutboxMessageStatus[] Statuses = Enum.GetValues<OutboxMessageStatus>();

    protected override async Task OnInitializedAsync()
    {
        ReadQueryParameters();
        await LoadFilterOptionsAsync();
        await LoadDataAsync();
    }

    private void ReadQueryParameters()
    {
        var uri = new Uri(navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (int.TryParse(query["page"], out var page) && page > 0)
        {
            _pageIndex = page;
        }

        _request = new MessageOutboxRequest
        {
            Keyword = query["keyword"],
            MessageType = query["type"],
            Exchange = query["exchange"],
            RoutingKey = query["routingKey"],
            Source = query["source"],
            Status = Enum.TryParse<OutboxMessageStatus>(query["status"], out var status)
                ? status
                : null,
            StartTime = DateTime.TryParse(query["start"], out var start) ? start : null,
            EndTime = DateTime.TryParse(query["end"], out var end) ? end : null,
        };
    }

    private void UpdateUrl()
    {
        var baseUri = navigation.ToAbsoluteUri("/message-outbox").GetLeftPart(UriPartial.Path);
        var queryParams = new List<string>();

        AddQuery(queryParams, "page", _pageIndex > 1 ? _pageIndex.ToString() : null);
        AddQuery(queryParams, "keyword", _request.Keyword);
        AddQuery(queryParams, "status", _request.Status?.ToString());
        AddQuery(queryParams, "type", _request.MessageType);
        AddQuery(queryParams, "exchange", _request.Exchange);
        AddQuery(queryParams, "routingKey", _request.RoutingKey);
        AddQuery(queryParams, "source", _request.Source);
        AddQuery(queryParams, "start", _request.StartTime?.ToString("yyyy-MM-ddTHH:mm:ss"));
        AddQuery(queryParams, "end", _request.EndTime?.ToString("yyyy-MM-ddTHH:mm:ss"));

        var url = queryParams.Count > 0 ? $"{baseUri}?{string.Join("&", queryParams)}" : baseUri;
        navigation.NavigateTo(url, false);
    }

    private static void AddQuery(List<string> queryParams, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            queryParams.Add($"{key}={Uri.EscapeDataString(value)}");
        }
    }

    private async Task LoadFilterOptionsAsync()
    {
        try
        {
            _filterOptions =
                await messageOutboxService.GetFilterOptionsAsync()
                ?? new MessageOutboxFilterOptionsResponse();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载筛选项失败：{ex.Message}", ToastType.Warning);
        }
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var result = await messageOutboxService.GetPageAsync(_request, _pageIndex, PageSize);
            _items = result.ToList();
            _totalCount = result.TotalItemCount;
            _totalPages = result.TotalPageCount;
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载消息队列失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SearchAsync()
    {
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task ReloadAsync()
    {
        await LoadDataAsync();
    }

    private async Task ResetAsync()
    {
        _request = new MessageOutboxRequest();
        _pageIndex = 1;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task UseStatusAsync(OutboxMessageStatus status)
    {
        _request.Status = _request.Status == status ? null : status;
        await SearchAsync();
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SearchAsync();
        }
    }

    private async Task HandlePageChanged(int page)
    {
        if (page < 1 || page > _totalPages || page == _pageIndex)
        {
            return;
        }

        _pageIndex = page;
        UpdateUrl();
        await LoadDataAsync();
    }

    private async Task DeleteAsync(MessageOutboxResponse item)
    {
        var confirmed = await dialogService.ConfirmAsync(
            $"确定删除消息 {item.MessageId} 吗？删除后不能恢复。",
            "删除 Outbox 记录"
        );
        if (!confirmed)
        {
            return;
        }

        try
        {
            await messageOutboxService.DeleteAsync(item.Id);
            dialogService.Toast("Outbox 记录已删除", ToastType.Success);

            if (_items.Count == 1 && _pageIndex > 1)
            {
                _pageIndex--;
                UpdateUrl();
            }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"删除失败：{ex.Message}", ToastType.Error);
        }
    }

    private async Task ReconsumeAsync(MessageOutboxResponse item)
    {
        if (!CanReconsume(item))
        {
            dialogService.Toast(
                $"当前状态 {GetStatusText(item.Status)} 不允许手动重新消费",
                ToastType.Warning
            );
            return;
        }

        var confirmed = await dialogService.ConfirmAsync(
            $"确定将消息 {item.MessageId} 重新入队消费吗？",
            "重新消费 Outbox 消息"
        );
        if (!confirmed)
        {
            return;
        }

        _reconsumingId = item.Id;
        StateHasChanged();

        try
        {
            await messageOutboxService.ReconsumeAsync(item.Id);
            dialogService.Toast("消息已重新入队消费", ToastType.Success);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"重新消费失败：{ex.Message}", ToastType.Error, 5000);
        }
        finally
        {
            _reconsumingId = null;
            StateHasChanged();
        }
    }

    private async Task ShowDetailsAsync(MessageOutboxResponse item)
    {
        await dialogService.ShowComponentAsync("Outbox 详情", BuildDetails(item), "920px");
    }

    private static RenderFragment BuildDetails(MessageOutboxResponse item)
    {
        return builder =>
        {
            var seq = 0;
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "message-outbox-detail");

            AddDetail(builder, ref seq, "记录 ID", item.Id);
            AddDetail(builder, ref seq, "消息 ID", item.MessageId);
            AddDetail(builder, ref seq, "消息类型", item.MessageType);
            AddDetail(builder, ref seq, "Exchange", item.Exchange);
            AddDetail(builder, ref seq, "路由键", item.RoutingKey);
            AddDetail(builder, ref seq, "来源", item.Source);
            AddDetail(builder, ref seq, "状态", GetStatusText(item.Status));
            AddDetail(builder, ref seq, "发布尝试", $"{item.PublishAttempts} 次");
            AddDetail(builder, ref seq, "最后发布尝试", FormatTime(item.LastPublishAttemptAt));
            AddDetail(builder, ref seq, "下次发布重试", FormatTime(item.NextPublishRetryAt));
            AddDetail(builder, ref seq, "发布时间", FormatTime(item.SentAt));
            AddDetail(builder, ref seq, "发布错误", item.LastPublishError, true);
            AddDetail(builder, ref seq, "消费尝试", $"{item.ConsumeAttempts} 次");
            AddDetail(builder, ref seq, "最后消费尝试", FormatTime(item.LastConsumeAttemptAt));
            AddDetail(builder, ref seq, "下次消费重试", FormatTime(item.NextConsumeRetryAt));
            AddDetail(builder, ref seq, "消费时间", FormatTime(item.ConsumedAt));
            AddDetail(builder, ref seq, "消费错误", item.LastConsumeError, true);
            AddDetail(builder, ref seq, "创建时间", FormatTime(item.CreateTime));
            AddDetail(builder, ref seq, "更新时间", FormatTime(item.LastUpdateTime));
            AddDetail(builder, ref seq, "Payload", item.Payload, true, true);

            builder.CloseElement();
        };
    }

    private static void AddDetail(
        RenderTreeBuilder builder,
        ref int seq,
        string label,
        string? value,
        bool wide = false,
        bool code = false
    )
    {
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(
            seq++,
            "class",
            wide
                ? "message-outbox-detail__item message-outbox-detail__item--wide"
                : "message-outbox-detail__item"
        );
        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "message-outbox-detail__label");
        builder.AddContent(seq++, label);
        builder.CloseElement();
        builder.OpenElement(seq++, code ? "pre" : "strong");
        builder.AddAttribute(seq++, "class", "message-outbox-detail__value");
        builder.AddContent(seq++, string.IsNullOrWhiteSpace(value) ? "-" : value);
        builder.CloseElement();
        builder.CloseElement();
    }

    private static string FormatTime(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    private static string GetStatusText(OutboxMessageStatus status)
    {
        return status switch
        {
            OutboxMessageStatus.Pending => "待发布",
            OutboxMessageStatus.Sent => "已发布",
            OutboxMessageStatus.Consumed => "已消费",
            OutboxMessageStatus.PublishFailed => "发布失败",
            OutboxMessageStatus.ConsumeFailed => "消费失败",
            _ => status.ToString(),
        };
    }

    private static string GetStatusDescription(OutboxMessageStatus status)
    {
        return status switch
        {
            OutboxMessageStatus.Pending => "等待投递",
            OutboxMessageStatus.Sent => "等待消费",
            OutboxMessageStatus.Consumed => "处理完成",
            OutboxMessageStatus.PublishFailed => "投递异常",
            OutboxMessageStatus.ConsumeFailed => "消费异常",
            _ => "未知状态",
        };
    }

    private static string GetStatusIcon(OutboxMessageStatus status)
    {
        return status switch
        {
            OutboxMessageStatus.Pending => "fas fa-hourglass-half",
            OutboxMessageStatus.Sent => "fas fa-paper-plane",
            OutboxMessageStatus.Consumed => "fas fa-circle-check",
            OutboxMessageStatus.PublishFailed => "fas fa-triangle-exclamation",
            OutboxMessageStatus.ConsumeFailed => "fas fa-circle-xmark",
            _ => "fas fa-circle-question",
        };
    }

    private static string GetStatusClass(OutboxMessageStatus status)
    {
        return status switch
        {
            OutboxMessageStatus.Pending => "message-outbox__status--pending",
            OutboxMessageStatus.Sent => "message-outbox__status--sent",
            OutboxMessageStatus.Consumed => "message-outbox__status--consumed",
            OutboxMessageStatus.PublishFailed => "message-outbox__status--publish-failed",
            OutboxMessageStatus.ConsumeFailed => "message-outbox__status--consume-failed",
            _ => "",
        };
    }

    private static bool CanReconsume(MessageOutboxResponse item)
    {
        return item.Status is OutboxMessageStatus.Sent or OutboxMessageStatus.ConsumeFailed;
    }

    private bool IsReconsuming(MessageOutboxResponse item)
    {
        return _reconsumingId == item.Id;
    }

    private string GetReconsumeIcon(MessageOutboxResponse item)
    {
        return IsReconsuming(item) ? "fas fa-spinner fa-spin" : "fas fa-sync-alt";
    }

    private string GetReconsumeText(MessageOutboxResponse item)
    {
        return IsReconsuming(item) ? "处理中" : "重新消费";
    }

    private string GetSummaryClass(OutboxMessageStatus status)
    {
        var activeClass = _request.Status == status ? "message-outbox__summary-item--active" : "";
        return $"{GetStatusClass(status)} {activeClass}";
    }
}
