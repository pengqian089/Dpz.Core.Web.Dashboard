using System.Text.Json.Serialization;
using Dpz.Core.Web.Dashboard.Models.Seo;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class PageRouteParameterDefinitionResponse
{
    public required string Name { get; set; }

    public string? TypeName { get; set; }

    [JsonConverter(typeof(EnumConverter<PageRouteParameterSource>))]
    public PageRouteParameterSource Source { get; set; }

    public bool IsOptional { get; set; }
}
