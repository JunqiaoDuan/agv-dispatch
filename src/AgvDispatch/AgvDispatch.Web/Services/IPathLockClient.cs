using AgvDispatch.Shared.DTOs.PathLocks;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 路径锁定 API 客户端接口
/// </summary>
public interface IPathLockClient
{
    /// <summary>
    /// 获取当前已放行的通道列表
    /// </summary>
    Task<List<ActiveChannelDto>> GetActiveChannelsAsync();

    /// <summary>
    /// 获取指定通道的详细信息
    /// </summary>
    /// <param name="channelName">通道名称</param>
    Task<ChannelDetailDto?> GetChannelDetailAsync(string channelName);

    /// <summary>
    /// 手动释放指定通道(仅释放已取消任务的锁定)
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <returns>成功释放的锁定数量</returns>
    Task<int> ReleaseChannelAsync(string channelName);
}
