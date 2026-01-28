using AgvDispatch.Shared.DTOs.PathLocks;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 路径锁定服务接口
/// </summary>
public interface IPathLockService
{
    /// <summary>
    /// 申请路径锁定
    /// </summary>
    /// <param name="fromStationCode">起始站点编号</param>
    /// <param name="toStationCode">目标站点编号</param>
    /// <param name="agvCode">小车编号</param>
    /// <param name="taskId">任务ID</param>
    /// <returns>是否批准及拒绝原因</returns>
    Task<(bool Approved, string? Reason)> RequestLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode,
        Guid taskId);

    /// <summary>
    /// 清理AGV的所有锁定（用于AGV离线或任务取消）
    /// </summary>
    /// <param name="agvCode">小车编号</param>
    Task ClearAgvLocksAsync(string agvCode);

    /// <summary>
    /// 获取当前已放行的通道列表
    /// </summary>
    /// <returns>已放行通道列表</returns>
    Task<List<ActiveChannelDto>> GetActiveChannelsAsync();

    /// <summary>
    /// 获取指定通道的详细信息
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <returns>通道详细信息</returns>
    Task<ChannelDetailDto?> GetChannelDetailAsync(string channelName);

    /// <summary>
    /// 手动释放指定通道(仅释放已取消任务的锁定)
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <returns>成功释放的锁定数量</returns>
    Task<int> ReleaseChannelAsync(string channelName);
}
