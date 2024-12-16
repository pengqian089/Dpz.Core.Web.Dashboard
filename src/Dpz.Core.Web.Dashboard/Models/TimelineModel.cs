using System;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class TimelineModel
    {
        public string Id { get; set; }
        
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// 时间节点
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 连接按钮的链接
        /// </summary>
        public string More { get; set; }
        
        /// <summary>
        /// 作者
        /// </summary>
        public UserInfo Author { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        public bool ShowContent { get; set; } = false;
    }
}