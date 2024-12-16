using System;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class UserInfo
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime? LastAccessTime { get; set; }

        /// <summary>
        /// 个性签名
        /// </summary>
        public string Sign { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonConverter(typeof(EnumConverter<Sex>))]
        public Sex Sex { get; set; }

         [JsonConverter(typeof(EnumNullableConverter<Permissions>))]
        public Permissions? Permissions { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? Enable { get; set; }

        public string Key { get; set; }
    }
}