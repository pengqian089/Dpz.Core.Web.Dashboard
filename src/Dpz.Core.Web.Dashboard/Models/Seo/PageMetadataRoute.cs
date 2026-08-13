using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models.Seo;

public class PageMetadataRoute
{
    public string? Area { get; set; }

    public string? Controller { get; set; }

    public string? Action { get; set; }

    public Dictionary<string, string> Parameters { get; set; } = [];
}
