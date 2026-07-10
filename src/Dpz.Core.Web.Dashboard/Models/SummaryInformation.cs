using System;
using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models;

public class SummaryInformation
{
    /// <summary>
    /// 最新日志
    /// </summary>
    public string? LatestLogs { get; set; }

    /// <summary>
    /// 汇总信息更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 文章总数
    /// </summary>
    public int ArticleTotalCount { get; set; }

    /// <summary>
    /// 今日文章数量
    /// </summary>
    public int TodayArticleCount { get; set; }

    /// <summary>
    /// Banner
    /// </summary>
    public List<PictureResponseModel> Banner { get; set; } = [];

    /// <summary>
    /// 最新文章
    /// </summary>
    public List<ArticleMiniResponse> LatestArticles { get; set; } = [];

    /// <summary>
    /// 今日访问次数
    /// </summary>
    public List<AccessSummary> TodayAccessNumber { get; set; } = [];

    /// <summary>
    /// 近7天访问次数
    /// </summary>
    public List<AccessSummary> WeekAccessNumber { get; set; } = [];

    /// <summary>
    /// 访问日志统计信息
    /// </summary>
    public AccessLogStatistics AccessLogStatistics { get; set; } = new();
}
