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
    /// 释放路径锁定
    /// </summary>
    /// <param name="fromStationCode">起始站点编号</param>
    /// <param name="toStationCode">目标站点编号</param>
    /// <param name="agvCode">小车编号</param>
    Task ReleaseLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode);

    /// <summary>
    /// 清理AGV的所有锁定（用于AGV离线或任务取消）
    /// </summary>
    /// <param name="agvCode">小车编号</param>
    Task ClearAgvLocksAsync(string agvCode);
}
