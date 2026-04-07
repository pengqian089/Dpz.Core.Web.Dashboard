namespace Dpz.Core.Web.Dashboard.Models.Request;

public abstract class PaginationRequest
{
    public int PageIndex { get; set; }
    
    public int PageSize { get; set; }
}