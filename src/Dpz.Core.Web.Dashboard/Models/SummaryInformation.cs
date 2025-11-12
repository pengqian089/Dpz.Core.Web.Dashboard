using System;
using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class SummaryInformation
    {
        /// <summary>
        /// 最新日志
        /// </summary>
        public string LatestLogs { get; set; }

        /// <summary>
        /// 文章总数
        /// </summary>
        public int ArticleTotalCount { get; set; }

        /// <summary>
        /// 热榜总回复数
        /// </summary>
        [Obsolete]
        public int TopicCommentTotalCount { get; set; }

        /// <summary>
        /// 今日文章数量
        /// </summary>
        public int TodayArticleCount { get; set; }

        /// <summary>
        /// Banner
        /// </summary>
        public List<PictureModel> Banner { get; set; }

        /// <summary>
        /// 最新文章
        /// </summary>
        public List<ArticleModel> LatestArticles { get; set; }

        /// <summary>
        /// 今日访问次数
        /// </summary>
        public List<AccessSummary> TodayAccessNumber { get; set; }

        /// <summary>
        /// 近7天访问次数
        /// </summary>
        public List<AccessSummary> WeekAccessNumber { get; set; }
    }
}
