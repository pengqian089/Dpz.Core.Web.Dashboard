using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models.Request;

public class SeoSaveRequest
{
    /// <summary>
    /// 可选，编辑时传入
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 页面标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 关键词
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 其他meta
    /// </summary>
    public Dictionary<string, string> Metas { get; set; } = [];

    /// <summary>
    /// 关联关系，至少 controller + action
    /// </summary>
    [MinLength(2)]
    public List<string> Relations { get; set; } = [];
}
