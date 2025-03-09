using System;
using System.Text.Json.Serialization;
using Dpz.Core.EnumLibrary;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Models.Response;

public class AccountTokenResponse
{
    public string Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// token 使用平台
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TokenPlatform>))]
    public TokenPlatform Platform { get; set; }

    /// <summary>
    /// 刷新Token
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// 刷新Token过期时间
    /// </summary>
    public DateTime RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// 首次生成时间
    /// </summary>
    public DateTime FirstGenerateTime { get; set; }

    /// <summary>
    /// 生成时的 IP 地址
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// 生成时的 User-Agent
    /// </summary>
    public string UserAgent { get; set; }
    
    /// <summary>
    /// 是否使用
    /// </summary>
    public bool? Used { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool ShowToken { get; set; }

    public string ShowTokenIcon { get; set; } = Icons.Material.Filled.Visibility;
}