namespace Dpz.Core.Web.Management.Models;

public class InterceptRuleModel
{
    public required string Id { get; set; }

    public InterceptRuleType Type { get; set; }

    public string Pattern { get; set; } = string.Empty;

    public string? Key { get; set; }
}
