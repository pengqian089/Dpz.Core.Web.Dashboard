using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dpz.Core.Web.Dashboard.Models.Request;

public class SaveDynamicRequest
{
    /// <summary>
    /// HTML 内容
    /// </summary>
    public HtmlContent HtmlContent { get; set; }

    /// <summary>
    /// 样式 内容
    /// </summary>
    public SortedDictionary<int, StyleContent> StyleContents { get; set; } = [];

    /// <summary>
    /// 脚本 内容
    /// </summary>
    public SortedDictionary<int, ScriptContent> ScriptContents { get; set; } = [];
}