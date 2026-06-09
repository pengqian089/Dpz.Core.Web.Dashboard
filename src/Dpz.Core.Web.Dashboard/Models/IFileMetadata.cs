namespace Dpz.Core.Web.Dashboard.Models;

/// <summary>
/// 文件元信息。
/// </summary>
public interface IFileMetadata
{
    /// <summary>
    /// 文件 URL 地址。
    /// </summary>
    string Url { get; }

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    long Size { get; }
}
