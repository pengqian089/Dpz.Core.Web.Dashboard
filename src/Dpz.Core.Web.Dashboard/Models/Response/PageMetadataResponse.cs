using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dpz.Core.Web.Dashboard.Models.Seo;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class PageMetadataResponse
{
    public string? Id { get; set; }

    /// <summary>
    /// 标准化关联路径（小写）
    /// </summary>
    public string? RelationPath { get; set; }

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
    [JsonConverter(typeof(EnumConverter<PageMetadataInheritanceMode>))]
    public PageMetadataInheritanceMode InheritanceMode { get; set; } =
        PageMetadataInheritanceMode.Inherit;

    /// <summary>
    /// 结构化路由关联
    /// </summary>
    public PageMetadataRoute? Route { get; set; }

    /// <summary>
    /// 标准化路由键
    /// </summary>
    public string? RouteKey { get; set; }

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
    /// 关联，唯一 {controller,action,id...}。仅用于兼容旧数据。
    /// </summary>
    public List<string> Relations { get; set; } = [];
}
