using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BlazorMonaco.Editor;
using Dpz.Core.EnumLibrary;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Helper;

public static class AppTools
{
    [JSInvokable]
    public static string GetWebApiHost()
    {
        return Program.BaseAddress;
    }

    public static Dictionary<int, string> PictureTypes =>
        typeof(PictureType).GetFields()
            .Where(x => x.IsPublic && x.IsStatic)
            .ToDictionary(x => (int)x.GetValue(null), x => x.Name);

    /// <summary>
    /// 客户端最大读取文件大小 unit byte
    /// </summary>
    public const long MaxFileSize = 1024 * 1024 * 100;

    /// <summary>
    /// 图片扩展名
    /// </summary>
    public static string[] ImageExtensions = ["jpg", "jpge", "png", "gif", "webp", "svg", "tiff", "psd", "bmp", "jiff"];

    /// <summary>
    /// 显示文件大小
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string FileSize(this long length)
    {
        var sizeText = length + " bytes";
        if (length > 1024m && length < 1024m * 1024m * 1)
        {
            sizeText = (length / 1024m).ToString("F") + " KB";
        }
        else if (length > 1024m * 1024m * 1 && length < 1024m * 1024m * 1024m)
        {
            sizeText = (length / 1024m / 1024m).ToString("F") + " MB";
        }
        else if (length > 1024m * 1024m * 1024m)
        {
            sizeText = (length / 1024m / 1024m / 1024m).ToString("F") + " GB";
        }

        return sizeText;
    }

    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public static void CopyTo<T>(this T source, out T destination) where T : class, new()
    {
        destination = default;
        var json = JsonSerializer.Serialize(source);
        destination = JsonSerializer.Deserialize<T>(json);
    }

    public static string TimeAgo(this DateTime time)
    {
        var ts = new TimeSpan(DateTime.UtcNow.Ticks - time.ToUniversalTime().Ticks);
        var delta = Math.Abs(ts.TotalSeconds);

        switch (delta)
        {
            case < 60:
                return ts.Seconds == 1 ? "刚刚" : ts.Seconds + "秒前";
            case < 60 * 2:
                return "1分钟前";
            case < 45 * 60:
                return ts.Minutes + "分钟前";
            case < 90 * 60:
                return "1小时前";
            case < 24 * 60 * 60:
                return ts.Hours + "小时前";
            case < 48 * 60 * 60:
                return "昨天";
            case < 30 * 24 * 60 * 60:
                return $"约{ts.Days}天前";
            case < 12 * 30 * 24 * 60 * 60:
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "约一个月前" : $"约{months}个月前";
            }
            default:
            {
                var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "大约1年前" : $"大约{years}年前";
            }
        }
    }

    public static StandaloneEditorConstructionOptions EditorConstructionOptions(this StandaloneCodeEditor editor,
        string language, string value, bool? isReadonly = null)
    {
        return new StandaloneEditorConstructionOptions
        {
            Theme = "vs-dark",
            Language = language,
            GlyphMargin = true,
            Value = value ?? "",
            AutomaticLayout = true,
            ReadOnly = isReadonly
        };
    }

    public static StandaloneEditorConstructionOptions HtmlEditorOptions(this StandaloneCodeEditor editor, string value)
    {
        return editor.EditorConstructionOptions("html", value);
    }

    public static StandaloneEditorConstructionOptions MarkdownEditorOptions(this StandaloneCodeEditor editor,
        string value)
    {
        Console.WriteLine($"MarkdownEditorOptions has value:{!string.IsNullOrEmpty(value)}");
        return editor.EditorConstructionOptions("markdown", value);
    }
}