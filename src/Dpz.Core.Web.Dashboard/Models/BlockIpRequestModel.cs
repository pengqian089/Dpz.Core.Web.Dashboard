using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models;

public class BlockIpRequestModel
{
    [Required(ErrorMessage = "请输入 IP 地址")]
    public string Ip { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "封禁时长须为 1 至 1440 分钟")]
    public int Minutes { get; set; } = 30;
}
