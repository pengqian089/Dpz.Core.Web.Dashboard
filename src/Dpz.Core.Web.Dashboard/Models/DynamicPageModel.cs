using System;

namespace Dpz.Core.Web.Dashboard.Models
{
    public class DynamicPageModel
    {
        public string Id { get; set; }
        
        public string HtmlContent { get; set; }
        
        public UserInfo Creator { get; set; }
        
        public DateTime CreateTime { get; set; }
        
        public DateTime LastUpdateTime { get; set; }
    }
}