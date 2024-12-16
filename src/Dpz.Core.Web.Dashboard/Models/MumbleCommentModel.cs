using System;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class MumbleCommentModel
    {
        /// <summary>
        /// 回复ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string Comment { get; set; }
        
        /// <summary>
        /// 回复人头像，如果为空则为匿名用户
        /// </summary>
        public string Avatar { get; set; }
        
        /// <summary>
        /// 回复人昵称
        /// </summary>
        public string NickName { get; set; }
        
        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime CommentTime { get; set; }
    }
}