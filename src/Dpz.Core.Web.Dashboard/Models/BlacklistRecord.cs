namespace Dpz.Core.Web.Dashboard.Models;

public class BlacklistRecord
{
    public required string Id { get; set; }

    public required string RequestMethod { get; set; }

    public required string RequestPath { get; set; }

    public string[] IpAddresses { get; set; } = [];

    public string[] UserAgents { get; set; } = [];

    public int AccessCount { get; set; }
}
