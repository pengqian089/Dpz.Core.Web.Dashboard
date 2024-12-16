using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    /// <summary>
    /// 身份认证
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// 当前登录的用户
        /// </summary>
        AppUser User { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="pinCode"></param>
        /// <returns></returns>
        Task<(bool result,string message)> SignAsync(string account,string password,string pinCode);

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        Task SignOutAsync();

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        Task<bool> RefreshTokenAsync(string token,string refreshToken);
    }
}