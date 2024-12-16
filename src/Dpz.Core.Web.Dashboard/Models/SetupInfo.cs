namespace Dpz.Core.Web.Dashboard.Models;

public class SetupInfo
{
    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 手动输入
    /// </summary>
    public string ManualEntryKey { get; set; }

    /// <summary>
    /// 二维码 Base64
    /// </summary>
    public string QrCodeSetupImageUrl { get; set; }
}