using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IVideoService
{
    /// <summary>
    /// 获取视频列表
    /// </summary>
    /// <returns></returns>
    Task<IList<VideoModel>> GetVideosAsync();

    /// <summary>
    /// 保存视频信息
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task SaveVideoInformationAsync(VideoModel model);

    /// <summary>
    /// 获取视频元信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<VideoMetaDataModel> GetVideoMetadataAsync(string id);

    /// <summary>
    /// 设置视频截图
    /// </summary>
    /// <param name="id"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    Task SetVideoScreenshotAsync(string id, double seconds);
}