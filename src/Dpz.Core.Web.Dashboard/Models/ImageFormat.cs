using System.Text.Json.Serialization;
using Dpz.Core.Web.Dashboard.Models.Enum;

namespace Dpz.Core.Web.Dashboard.Models;

/// <summary>
/// 图片格式识别结果。
/// </summary>
public readonly record struct ImageFormat(
    [property: JsonConverter(typeof(EnumConverter<ImageFormatKind>))] ImageFormatKind Kind,
    string DefaultMimeType,
    string DefaultExtension
)
{
    /// <summary>
    /// 未识别的格式占位。
    /// </summary>
    public static readonly ImageFormat Unknown = new(
        ImageFormatKind.Unknown,
        "application/octet-stream",
        ""
    );

    /// <summary>
    /// 是否识别到具体格式。
    /// </summary>
    public bool IsKnown => Kind != ImageFormatKind.Unknown;

    /// <summary>
    /// 图片格式名称。
    /// </summary>
    public string Name => DefaultExtension;

    public static readonly ImageFormat Jpeg = new(ImageFormatKind.Jpeg, "image/jpeg", "jpg");
    public static readonly ImageFormat Png = new(ImageFormatKind.Png, "image/png", "png");
    public static readonly ImageFormat Gif = new(ImageFormatKind.Gif, "image/gif", "gif");
    public static readonly ImageFormat Bmp = new(ImageFormatKind.Bmp, "image/bmp", "bmp");
    public static readonly ImageFormat Webp = new(ImageFormatKind.Webp, "image/webp", "webp");
    public static readonly ImageFormat Tiff = new(ImageFormatKind.Tiff, "image/tiff", "tiff");
    public static readonly ImageFormat Heic = new(ImageFormatKind.Heic, "image/heic", "heic");
    public static readonly ImageFormat Avif = new(ImageFormatKind.Avif, "image/avif", "avif");
    public static readonly ImageFormat Ico = new(ImageFormatKind.Ico, "image/x-icon", "ico");
}
