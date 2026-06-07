using System;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class AppNotificationHandle
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public AppNotificationOptions Options { get; set; } = new();

    public Action<string>? UpdateContent { get; set; }
    public Action<string>? UpdateTitle { get; set; }
    public Action<double[]>? UpdateProgress { get; set; }
    public Action<AppFeedbackLevel>? UpdateLevel { get; set; }
    public Action? Close { get; set; }
}
