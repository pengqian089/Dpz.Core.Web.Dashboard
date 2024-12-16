﻿using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service
{
    public interface IAccountService
    {
        Task<IPagedList<UserInfo>> GetPageAsync(string account = null, int pageIndex = 1, int pageSize = 10);

        Task CreateAccountAsync(string account, string name,string password = null);

        Task<UserInfo> GetUserAsync(string account);

        Task EnableAsync(string account);

        Task ChangePasswordAsync(string account, string password);

        Task<bool> ExistsAsync(string account);
    }
} 