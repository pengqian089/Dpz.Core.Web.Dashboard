namespace Dpz.Core.Web.Dashboard.Models;

public class AccessRecord
{
    public string Id { get; set; }

    public string Level { get; set; }

    public string Date { get; set; }

    public string RenderedMessage { get; set; }
}

public class AccessSummary
{
    public int Count { get; set; }

    public string Date { get; set; }
}