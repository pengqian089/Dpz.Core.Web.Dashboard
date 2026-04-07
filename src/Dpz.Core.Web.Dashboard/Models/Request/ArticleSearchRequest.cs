namespace Dpz.Core.Web.Dashboard.Models.Request;

public class ArticleSearchRequest : PaginationRequest
{
    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string? Author { get; set; }
}
