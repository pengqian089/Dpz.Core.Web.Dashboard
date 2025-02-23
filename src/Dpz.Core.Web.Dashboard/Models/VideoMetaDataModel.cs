namespace Dpz.Core.Web.Dashboard.Models;

public class VideoMetaDataModel
{
    /// <summary>
    /// 视频时长
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// 分片时长
    /// </summary>
    public double[] Points { get; set; }
}
