using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

    public static Lazy<Dictionary<int, string>> PictureTypes =>
        new(() =>
            typeof(PictureType)
                .GetFields()
                .Where(x => x is { IsPublic: true, IsStatic: true })
                .Select(x =>
                {
                    var key = -1;
                    var typeValue = (PictureType?)x.GetValue(null);
                    if (typeValue.HasValue)
                    {
                        key = (int)typeValue.Value;
                    }
                    return new KeyValuePair<int, string>(key, x.Name);
                })
                .Where(x => x.Key >= 0)
                .ToDictionary(x => x.Key, x => x.Value)
        );

    /// <summary>
    /// 客户端最大读取文件大小 unit byte
    /// </summary>
    public static long MaxFileSize => 1024 * 1024 * 100;

    /// <summary>
    /// 图片扩展名
    /// </summary>
    public static string[] ImageExtensions =>
        ["jpg", "jpge", "png", "gif", "webp", "svg", "tiff", "psd", "bmp", "jiff"];

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
}
