using Dpz.Core.Web.Dashboard.Models.Request;

public class CodeFlatRequest : PaginationRequest
{
    /// <summary>
    /// 路径查询（精确匹配，多级路径）
    /// </summary>
    public string[]? PathSegments { get; set; }

    /// <summary>
    /// 名称查询（模糊匹配）
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 扩展名查询（精确匹配）
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public CodeFileSortField? SortField { get; set; }

    /// <summary>
    /// 是否降序排序
    /// </summary>
    public bool IsDescending { get; set; } = true;
}

/// <summary>
/// 源码文件排序字段
/// </summary>
public enum CodeFileSortField
{
    /// <summary>
    /// 按大小排序
    /// </summary>
    Size = 1,

    /// <summary>
    /// 按创建时间排序
    /// </summary>
    CreatedTime = 2,

    /// <summary>
    /// 按最后修改时间排序
    /// </summary>
    LastWriteTime = 3,

    /// <summary>
    /// 按最后更新时间排序
    /// </summary>
    LastUpdateTime = 4,

    /// <summary>
    /// 按AI分析时间排序
    /// </summary>
    AiAnalyzeTime = 5,
}
