using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ICommunityService
{
    Task<SummaryInformation> GetSummaryAsync();

    /// <summary>
    /// 获取页脚内容
    /// </summary>
    /// <returns></returns>
    Task<string> GetFooterAsync();

    /// <summary>
    /// 保存页脚内容
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task SaveFooterAsync(string content);

    /// <summary>
    /// 获取robots.txt内容
    /// </summary>
    /// <returns></returns>
    Task<string> GetRobotsAsync();

    /// <summary>
    /// 保存robots.txt
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task SaveRobotsAsync(string content);
}
