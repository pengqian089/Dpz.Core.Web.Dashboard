using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class SystemNotificationHistoryResponse
{
    public string Id { get; set; } = "";

    public string? Message { get; set; }

    public string? Content { get; set; }

    public string? Text { get; set; }

    public DateTime? CreateTime { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? SendTime { get; set; }

    public DateTime? SentTime { get; set; }

    public DateTime? SentAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraProperties { get; set; }

    [JsonIgnore]
    public string DisplayId => !string.IsNullOrWhiteSpace(Id) ? Id : GetString("id");

    [JsonIgnore]
    public string DisplayMessage =>
        FirstNotEmpty(
            Message,
            Content,
            Text,
            GetString("message"),
            GetString("content"),
            GetString("text")
        );

    [JsonIgnore]
    public DateTime? DisplayTime =>
        CreateTime
        ?? CreatedTime
        ?? CreatedAt
        ?? SendTime
        ?? SentTime
        ?? SentAt
        ?? GetDateTime("createTime")
        ?? GetDateTime("createdTime")
        ?? GetDateTime("createdAt")
        ?? GetDateTime("sendTime")
        ?? GetDateTime("sentTime")
        ?? GetDateTime("sentAt");

    private string GetString(string key)
    {
        if (ExtraProperties == null)
        {
            return "";
        }

        foreach (var item in ExtraProperties)
        {
            if (!item.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString() ?? ""
                : item.Value.ToString();
        }

        return "";
    }

    private DateTime? GetDateTime(string key)
    {
        var value = GetString(key);
        return DateTime.TryParse(value, out var dateTime) ? dateTime : null;
    }

    private static string FirstNotEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return "";
    }
}
