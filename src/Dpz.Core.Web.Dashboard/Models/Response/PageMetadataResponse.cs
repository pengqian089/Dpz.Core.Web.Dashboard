using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class PageMetadataResponse
{
    public required string Id { get; set; }

    /// <summary>
    /// 标准化关联路径（小写）
    /// </summary>
    public string? RelationPath { get; set; }

    /// <summary>
    /// 非必要不要设置
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 关键字
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 其它元数据
    /// </summary>
    public Dictionary<string, string> Metas { get; set; } = [];

    /// <summary>
    /// 关联，唯一 {controller,action,id...}
    /// </summary>
    public List<string> Relations { get; set; } = [];
}
