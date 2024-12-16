namespace Dpz.Core.Web.Dashboard.Models
{
    public class PictureInfoModel
    {
        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 图片大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 资源地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 图片ID或者为API删除的提交地址 
        /// </summary>
        public string Delete { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string Ip { get; set; }
    }
}