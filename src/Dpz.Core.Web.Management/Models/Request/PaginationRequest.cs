namespace Dpz.Core.Web.Management.Models.Request;

public abstract class PaginationRequest
{
    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}
