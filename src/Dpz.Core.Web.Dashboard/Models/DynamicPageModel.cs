using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;

#nullable  enable


namespace Dpz.Core.Web.Dashboard.Models;

public class DynamicPageModel
{
    public string? Id { get; set; }

    public string? Content { get; set; }

    /// <summary>
    /// 脚本 地址
    /// </summary>
    public List<ScriptContent> Scripts { get; set; } = [];

    /// <summary>
    /// 样式 地址
    /// </summary>
    public List<StyleContent> Styles { get; set; } = [];
    
    /// <summary>
    /// Content-Type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<PageContentType>))]
    public PageContentType? ContentType { get; set; }

    /// <summary>
    /// content type
    /// </summary>
    public string? ContentTypeStr => ContentType?.ToString();

    public UserInfo? Creator { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
}

public class PageContent
{
    /// <summary>
    /// 页面名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Content-Type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<PageContentType>))]
    public PageContentType ContentType { get; set; }

    /// <summary>
    /// Content-Type 字符串
    /// </summary>
    public string ContentTypeStr => ContentType.ToString();
}

public class ScriptContent : PageContent
{
    public ScriptContent()
    {
        ContentType = PageContentType.JavaScript;
    }
}

public class StyleContent : PageContent
{
    public StyleContent()
    {
        ContentType = PageContentType.Css;
    }
}