using System;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class AudioModel
    {
        public string Id { get; set; }
    
        public string AccessUrl { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
        
        /// <summary>
        /// 时长
        /// </summary>
        public string Duration { get; set; }
        
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }
        
        /// <summary>
        /// 上传人
        /// </summary>
        public UserInfo Uploader { get; set; }
    }
}