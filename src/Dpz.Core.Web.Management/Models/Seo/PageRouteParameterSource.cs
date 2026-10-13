using System;

namespace Dpz.Core.Web.Management.Models.Seo;

[Flags]
public enum PageRouteParameterSource
{
    None = 0,
    Route = 1,
    Query = 2,
}
