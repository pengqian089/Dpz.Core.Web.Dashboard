using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;
using Dpz.Core.Web.Dashboard;

public class CodeFileSystemEntryResponse
{
    public required string Id { get; set; }

    /// <summary>
    /// 完整路径分段
    /// </summary>
    public required List<string> PathSegments { get; set; } = [];

    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 父级路径分段（根目录为空集合）
    /// </summary>
    public List<string> ParentPathSegments { get; set; } = [];

    /// <summary>
    /// 是否为目录
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// 扩展名（目录为空）
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// 大小（目录为空）
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// 文件哈希（目录为空）
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// 文件内容类型（目录为 Unknown）
    /// </summary>
    [JsonConverter(typeof(EnumConverter<CodeFileContentType>))]
    public CodeFileContentType CodeFileContentType { get; set; }

    /// <summary>
    /// 文件内容（目录为空）
    /// </summary>
    public string? FileContent { get; set; }

    /// <summary>
    /// 代码语言
    /// </summary>
    public string? CodeLanguage { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastWriteTime { get; set; }

    /// <summary>
    /// 最后更新时间（数据更新时间）
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// AI分析结果
    /// </summary>
    public string? AiAnalyzeResult { get; set; }

    /// <summary>
    /// AI分析时间
    /// </summary>
    public DateTime? AiAnalyzeTime { get; set; }

    /// <summary>
    /// AI分析对应的文件哈希
    /// </summary>
    public string? AiAnalyzeHash { get; set; }

    /// <summary>
    /// Readme内容
    /// </summary>
    public string? ReadmeContent { get; set; }
}
