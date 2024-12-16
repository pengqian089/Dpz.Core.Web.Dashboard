using System;
using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class TimelineCreateRequest
    {
        [Required(ErrorMessage = "请输入标题")]
        public string Title { get; set; }
        
        public string Content { get; set; }
        
        [Required(ErrorMessage = "请选择时间轴节点")]
        public DateTime? Date { get; set; }
        
        public string More { get; set; }
    }

    public class TimelineEditRequest : TimelineCreateRequest
    {
        public string Id { get; set; }
    }
}