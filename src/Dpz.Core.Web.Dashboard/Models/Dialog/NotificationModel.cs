using System;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class NotificationModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationOptions Options { get; set; } = new();

    // Methods to update state dynamically if needed
    public Action<string>? UpdateContent { get; set; }
    public Action<string>? UpdateTitle { get; set; }
    public Action<double[]>? UpdateProgress { get; set; }
    public Action<NotificationType>? UpdateType { get; set; }
    public Action? Close { get; set; }
}
