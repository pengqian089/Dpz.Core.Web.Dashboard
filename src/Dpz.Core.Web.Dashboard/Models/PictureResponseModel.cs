using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class PictureResponseModel
    {
        public string Id { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        public UserInfo Creator { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 图片描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图像类型
        /// </summary>
        [JsonConverter(typeof(EnumConverter<PictureCategory>))]
        public PictureCategory Category { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 访问地址
        /// </summary>
        public string AccessUrl { get; set; }

        /// <summary>
        /// 图片大小
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// MD5
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        /// 云储存上传时间
        /// </summary>
        public DateTime ObjectStorageUploadTime { get; set; }
    }

    public class ImageModel
    {
        /// <summary>
        /// 图片大小
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 文件ID
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// MD5
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        /// 访问地址
        /// </summary>
        public string Url { get; set; }
    }
}