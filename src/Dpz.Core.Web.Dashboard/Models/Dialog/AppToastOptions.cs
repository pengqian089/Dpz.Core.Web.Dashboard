namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class AppToastOptions
{
    public string Message { get; set; } = "";
    public AppFeedbackLevel Level { get; set; } = AppFeedbackLevel.Info;
    public int Duration { get; set; } = 3000;
}
