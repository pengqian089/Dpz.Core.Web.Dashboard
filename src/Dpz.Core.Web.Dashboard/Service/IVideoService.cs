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
}