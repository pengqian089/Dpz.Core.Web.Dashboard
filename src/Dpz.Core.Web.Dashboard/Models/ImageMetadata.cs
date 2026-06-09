namespace Dpz.Core.Web.Dashboard.Models;

/// <summary>
/// 图片元信息
/// </summary>
public readonly struct ImageMetadata : IFileMetadata
{
    /// <summary>
    /// 图片 URL 地址
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// 图片宽度（像素）
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// 图片高度（像素）
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    /// 图片帧数
    /// 未知 0，静态图 1
    /// </summary>
    public int Frames { get; init; }

    /// <summary>
    /// 图片哈希值
    /// </summary>
    public string? Hash { get; init; }

    /// <summary>
    /// 图片大小（字节）
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// 图片格式
    /// </summary>
    public ImageFormat Format { get; init; }
}
