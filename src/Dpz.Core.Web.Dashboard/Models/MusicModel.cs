using System;
using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class MusicModel
    {
        public string Id { get; set; }

        /// <summary>
        /// 音乐Url
        /// </summary>
        public string MusicUrl { get; set; }

        /// <summary>
        /// 歌词Url
        /// </summary>
        public string LyricUrl { get; set; }

        /// <summary>
        /// 音乐源文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 音乐标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 音乐时长
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// 音乐大小
        /// </summary>
        public long MusicLength { get; set; }

        /// <summary>
        /// 艺术家
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 封面ID
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        public List<string> Group { get; set; } = new();
    }
}