using System;
using Dpz.Core.EnumLibrary;

namespace Dpz.Core.Web.Dashboard.Models.Request;

#nullable enable

public class AccountLoginHistoryRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 登录方式
    /// </summary>
    public LoginMethod? Method { get; set; }

    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginResultStatus? Status { get; set; }

    /// <summary>
    /// 登录开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 登录结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
