namespace Dpz.Core.Web.Dashboard.Models;

public class CodeSaveModel
{
    /// <summary>
    /// 文件、目录所在的目录
    /// </summary>
    public string[] Path { get; set; }
        
    /// <summary>
    /// 文件、目录的名称
    /// </summary>
    public string Name { get; set; }
        
    /// <summary>
    /// 文件、目录的说明
    /// </summary>
    public string Note { get; set; }
}