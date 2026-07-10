using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models;

/// <summary>
/// 访问日志统计信息
/// </summary>
public class AccessLogStatistics
{
    /// <summary>
    /// 慢请求排行
    /// </summary>
    public List<SlowRequestSummary> SlowRequests { get; set; } = [];

    /// <summary>
    /// 热门页面排行
    /// </summary>
    public List<RequestPathSummary> PopularPages { get; set; } = [];

    /// <summary>
    /// 每小时流量统计
    /// </summary>
    public List<HourlyTrafficSummary> HourlyTraffic { get; set; } = [];

    /// <summary>
    /// 浏览器占比
    /// </summary>
    public List<BrowserUsageSummary> BrowserUsage { get; set; } = [];

    /// <summary>
    /// 来源排行
    /// </summary>
    public List<RefererSummary> Referers { get; set; } = [];
}

/// <summary>
/// 慢请求统计项
/// </summary>
public class SlowRequestSummary
{
    /// <summary>
    /// 请求发生时间
    /// </summary>
    public required string Time { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 请求耗时，单位毫秒
    /// </summary>
    public double Elapsed { get; set; }

    /// <summary>
    /// IP 地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }
}

/// <summary>
/// 请求路径访问统计项
/// </summary>
public class RequestPathSummary
{
    /// <summary>
    /// 请求路径
    /// </summary>
    public required string RequestPath { get; set; }

    /// <summary>
    /// 请求次数
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// 每小时流量统计项
/// </summary>
public class HourlyTrafficSummary
{
    /// <summary>
    /// 日期
    /// </summary>
    public required string Date { get; set; }

    /// <summary>
    /// 小时
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// 请求次数
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// 浏览器访问统计项
/// </summary>
public class BrowserUsageSummary
{
    /// <summary>
    /// 浏览器名称
    /// </summary>
    public required string Browser { get; set; }

    /// <summary>
    /// 请求次数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 请求占比
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// 来源访问统计项
/// </summary>
public class RefererSummary
{
    /// <summary>
    /// 来源
    /// </summary>
    public required string Referer { get; set; }

    /// <summary>
    /// 请求次数
    /// </summary>
    public int Count { get; set; }
}
