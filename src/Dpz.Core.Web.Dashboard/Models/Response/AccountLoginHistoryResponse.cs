using System;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;
using MudBlazor;
using MudBlazor.Utilities;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class AccountLoginHistoryResponse
{
    public string Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// SessionId
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// 登录方式
    /// </summary>
    [JsonConverter(typeof(EnumConverter<LoginMethod>))]
    public LoginMethod Method { get; set; }

    public string MethodStr =>
        Method switch
        {
            LoginMethod.Password => "密码登录",
            _ => throw new ArgumentOutOfRangeException(),
        };

    /// <summary>
    /// 登录状态
    /// </summary>
    [JsonConverter(typeof(EnumConverter<LoginResultStatus>))]
    public LoginResultStatus Status { get; set; }

    public string StatusStr =>
        Status switch
        {
            LoginResultStatus.Success => "登录成功",
            LoginResultStatus.AccountOrPasswordError => "账号或密码错误",
            LoginResultStatus.PinCodeError => "PIN码错误",
            LoginResultStatus.AccountLocked => "账号锁定",
            _ => throw new ArgumentOutOfRangeException(),
        };

    public Color Color =>
        Status switch
        {
            LoginResultStatus.Success => Color.Success,
            LoginResultStatus.AccountOrPasswordError => Color.Error,
            LoginResultStatus.PinCodeError => Color.Warning,
            LoginResultStatus.AccountLocked => Color.Primary,
            _ => throw new ArgumentOutOfRangeException(),
        };

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}
