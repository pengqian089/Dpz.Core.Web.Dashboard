using System.ComponentModel.DataAnnotations;

namespace Dpz.Core.Web.Dashboard.Models;

public class InterceptRuleEditModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "请选择拦截类型")]
    public InterceptRuleType Type { get; set; }

    [Required(ErrorMessage = "请输入匹配模式")]
    public string Pattern { get; set; } = string.Empty;

    public string? Key { get; set; }
}
