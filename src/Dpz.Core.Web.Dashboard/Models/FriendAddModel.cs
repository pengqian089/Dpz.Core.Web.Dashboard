using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models;

public class FriendAddModel
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "请输入名称")]
    public string Name { get; set; }
    
    /// <summary>
    /// 图标
    /// </summary>
    [Required(ErrorMessage = "请输入图标地址")]
    public string Avatar { get; set; }

    /// <summary>
    /// 链接
    /// </summary>
    [Required(ErrorMessage = "请输入链接地址")]
    public string Link { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
}