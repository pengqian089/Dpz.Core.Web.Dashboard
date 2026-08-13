using System.Collections.Generic;
using Dpz.Core.Web.Dashboard.Models.Seo;

namespace Dpz.Core.Web.Dashboard.Models.Request;

public class SeoSaveRequest
{
    /// <summary>
    /// 可选，编辑时传入
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 是否公共元数据
    /// </summary>
    public bool IsPublicMetadata { get; set; }

    /// <summary>
    /// 公共元数据是否应用到没有显式配置的页面
    /// </summary>
    public bool ApplyToUnconfiguredPages { get; set; }

    /// <summary>
    /// 页面元数据继承方式
    /// </summary>
    public PageMetadataInheritanceMode InheritanceMode { get; set; } =
        PageMetadataInheritanceMode.Inherit;

    /// <summary>
    /// 结构化路由关联
    /// </summary>
    public PageMetadataRoute? Route { get; set; }

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
    /// 关联关系，至少 controller + action。仅用于兼容旧数据，新保存不使用。
    /// </summary>
    public List<string> Relations { get; set; } = [];
}
