using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class PageRouteEndpointDefinitionResponse
{
    public string? RouteName { get; set; }

    public required string Template { get; set; }

    public List<string> HttpMethods { get; set; } = [];

    public Dictionary<string, string> Defaults { get; set; } = [];

    public Dictionary<string, string> RequiredValues { get; set; } = [];
}
