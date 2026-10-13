namespace Dpz.Core.Web.Management.Models.Dialog;

public class NotificationOptions
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public double[] Bars { get; set; } = [];
    public int AutoClose { get; set; } = 0;
    public NotificationType Type { get; set; } = NotificationType.Info;
}
