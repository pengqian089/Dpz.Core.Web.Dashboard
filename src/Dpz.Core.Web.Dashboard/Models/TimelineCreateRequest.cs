using System;
using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models;

public class TimelineCreateRequest
{
    [Required(ErrorMessage = "请输入标题")]
    public required string Title { get; set; }

    public string? Content { get; set; }

    [Required(ErrorMessage = "请选择时间轴节点")]
    public required DateTime Date { get; set; }

    public string? More { get; set; }
}

public class TimelineEditRequest : TimelineCreateRequest
{
    public required string Id { get; set; }
}
