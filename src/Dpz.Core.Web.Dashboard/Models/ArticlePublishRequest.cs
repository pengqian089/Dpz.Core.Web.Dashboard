using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models;

public class ArticlePublishRequest
{
    [Required(ErrorMessage = "请输入标题")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "请输入简介")]
    public string? Introduction { get; set; }

    [Required(ErrorMessage = "请选择标签")]
    public List<string> Tags { get; set; } = [];

    public string? Markdown { get; set; }

    public List<ImageMetadata> Images { get; set; } = [];
}
