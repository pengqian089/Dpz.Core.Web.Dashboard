using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Models.Seo;

namespace Dpz.Core.Web.Dashboard.Models.Request;

public class SeoPreviewRequest
{
    public required PageMetadataRoute Route { get; set; }

    public PageMetadataResponse? ExplicitMetadata { get; set; }
}
