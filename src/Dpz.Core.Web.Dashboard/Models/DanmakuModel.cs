using System;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class DanmakuModel
    {
        public string Id { get; set; }

        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// 分组
        /// </summary>
        public string Group { get; set; }
        
        /// <summary>
        /// 弹幕颜色
        /// </summary>
        public string Color { get; set; }
        
        /// <summary>
        /// 0为滚动 1 为顶部 2为底部
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// —弹幕文字大小。 0为小字 1为大字
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// 弹幕所出现的时间。 单位为分秒（十分之一秒）
        /// </summary>
        public decimal Time { get; set; }
        
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}