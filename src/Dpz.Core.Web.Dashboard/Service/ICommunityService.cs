using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface ICommunityService
    {
        Task<string> GetLogsAsync();

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
        /// 获取双因素绑定注册信息
        /// </summary>
        /// <returns></returns>
        Task<SetupInfo> GetTwoFactorSetupInfoAsync();

        /// <summary>
        /// 绑定双因素验证
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pinCode"></param>
        /// <returns></returns>
        Task BindTwoFactorAsync(string pinCode);

        /// <summary>
        /// 解除绑定双因素验证
        /// </summary>
        /// <param name="pinCode"></param>
        /// <returns></returns>
        Task UnbindTwoFactorAsync(string pinCode);

        /// <summary>
        /// 检测是否已绑定双因素验证
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckBindTwoFactorAsync();
    }
}