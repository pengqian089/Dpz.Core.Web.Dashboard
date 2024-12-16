using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dpz.Core.Web.Dashboard.Pages.Article;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class ArticlePublishRequest
    {
        [Required(ErrorMessage = "请输入标题")]
        public string Title { get; set; }

        [Required(ErrorMessage = "请输入简介")]
        public string Introduction { get; set; }

        public string Content { get; set; }

        [Required(ErrorMessage = "请选择标签")]
        public IEnumerable<string> Tags { get; set; }

        public string Markdown { get; set; }
    }
}