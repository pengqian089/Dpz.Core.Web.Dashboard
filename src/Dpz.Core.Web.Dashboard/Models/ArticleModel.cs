using System;
using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class ArticleModel
    {
        public string Id { get; set; }

        /// <summary>
        /// 文章标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// Html内容
        /// </summary>
        public string HtmlContent { get; set; }

        /// <summary>
        /// Markdown 内容
        /// </summary>
        public string Markdown { get; set; }

        /// <summary>
        /// 主图片
        /// </summary>
        public string MainImage { get; set; }

        /// <summary>
        /// 回复量
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 查看量
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 文章相关图片地址
        /// </summary>
        public List<string> ImagesAddress { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public UserInfo Author { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }
}