using System.Text.Json.Serialization;
using Dpz.Core.Web.Management.Models.Seo;

namespace Dpz.Core.Web.Management.Models.Response;

public class PageRouteParameterDefinitionResponse
{
    public required string Name { get; set; }

    public string? TypeName { get; set; }

    [JsonConverter(typeof(EnumConverter<PageRouteParameterSource>))]
    public PageRouteParameterSource Source { get; set; }

    public bool IsOptional { get; set; }
}
