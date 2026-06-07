namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class AppNotificationOptions
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public double[] Progress { get; set; } = [];
    public AppFeedbackLevel Level { get; set; } = AppFeedbackLevel.Info;
    public bool AutoClose { get; set; }
    public int Duration { get; set; } = 5000;
}
