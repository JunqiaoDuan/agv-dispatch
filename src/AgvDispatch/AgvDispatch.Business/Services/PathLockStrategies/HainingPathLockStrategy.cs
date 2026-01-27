using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services.PathLockStrategies;

/// <summary>
/// Haining 项目专用路径锁定策略
/// </summary>
/// <remarks>
/// 针对 Haining 项目的业务场景：
/// - 三辆小车
/// - 路线不是特别复杂
/// - 存在双向通道等特殊情况
/// - 使用手动配置的锁定规则，而非通用算法
/// </remarks>
public class HainingPathLockStrategy : IPathLockStrategy
{
    private readonly IRepository<TaskPathLock> _lockRepository;
    private readonly IRepository<Agv> _agvRepository;
    private readonly ILogger<HainingPathLockStrategy> _logger;

    public HainingPathLockStrategy(
        IRepository<TaskPathLock> lockRepository,
        IRepository<Agv> agvRepository,
        ILogger<HainingPathLockStrategy> logger)
    {
        _lockRepository = lockRepository;
        _agvRepository = agvRepository;
        _logger = logger;
    }

    public async Task<(bool Approved, string? Reason)> RequestLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode,
        Guid taskId)
    {
        _logger.LogInformation(
            "Haining专用策略：AGV {AgvCode} 申请锁定路径 {From} -> {To}，任务 {TaskId}",
            agvCode, fromStationCode, toStationCode, taskId);

        // TODO: 在此处添加 Haining 项目特定的锁定逻辑
        // 例如：
        // 1. 检查双向通道是否已被占用
        // 2. 检查关键路口是否有其他小车
        // 3. 根据特定站点对之间的冲突规则判断

        // 临时返回批准，待后续填充具体业务逻辑
        return (true, null);
    }

    public async Task ReleaseLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode)
    {
        _logger.LogInformation(
            "Haining专用策略：AGV {AgvCode} 释放锁定路径 {From} -> {To}",
            agvCode, fromStationCode, toStationCode);

        // TODO: 在此处添加 Haining 项目特定的释放逻辑
    }

    public async Task ClearAgvLocksAsync(string agvCode)
    {
        _logger.LogInformation(
            "Haining专用策略：清理 AGV {AgvCode} 的所有锁定",
            agvCode);

        // TODO: 在此处添加 Haining 项目特定的清理逻辑
    }
}
