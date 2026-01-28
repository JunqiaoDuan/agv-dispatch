namespace AgvDispatch.Business.Services.PathLockStrategies;

/// <summary>
/// 路径锁定策略接口（用于不同项目的专用锁定逻辑）
/// </summary>
public interface IPathLockStrategy
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
    /// 清理AGV的所有锁定
    /// </summary>
    /// <param name="agvCode">小车编号</param>
    Task ClearAgvLocksAsync(string agvCode);
}
