using System;
using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class PageRouteDefinitionResponse
{
    public string? Id { get; set; }

    public required string RouteKey { get; set; }

    public string? Area { get; set; }

    public required string Controller { get; set; }

    public required string Action { get; set; }

    public List<string> HttpMethods { get; set; } = [];

    public List<PageRouteEndpointDefinitionResponse> Endpoints { get; set; } = [];

    public List<PageRouteParameterDefinitionResponse> Parameters { get; set; } = [];

    public bool IsActive { get; set; }

    public DateTime LastScannedAt { get; set; }
}
