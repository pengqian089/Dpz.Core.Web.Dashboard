namespace Dpz.Core.Web.Dashboard.Models;

public class SettingModel
{
    /// <summary>
    /// 核心站点
    /// </summary>
    public string Core { get; set; }

    /// <summary>
    /// 主站
    /// </summary>
    public string Main { get; set; }

    /// <summary>
    /// 任务站点
    /// </summary>
    public string Job { get; set; }

    /// <summary>
    /// api站点
    /// </summary>
    public string Api { get; set; }

    /// <summary>
    /// 图片站点
    /// </summary>
    public string Image { get; set; }

    /// <summary>
    /// 消息站点
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// 静态文件站点
    /// </summary>
    public string Static { get; set; }

    /// <summary>
    /// cdn站点
    /// </summary>
    public string Cdn { get; set; }

    /// <summary>
    /// 自定义脚本站点
    /// </summary>
    public string Script { get; set; }
}