using Dpz.Core.Web.Management.Models.Response;
using Dpz.Core.Web.Management.Models.Seo;

namespace Dpz.Core.Web.Management.Models.Request;

public class SeoPreviewRequest
{
    public required PageMetadataRoute Route { get; set; }

    public PageMetadataResponse? ExplicitMetadata { get; set; }
}
