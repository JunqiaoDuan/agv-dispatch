using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services.PathLockStrategies;

/// <summary>
/// 默认路径锁定策略（使用通用算法）
/// </summary>
public class DefaultPathLockStrategy : IPathLockStrategy
{
    private readonly IRepository<TaskPathLock> _lockRepository;
    private readonly IRepository<Agv> _agvRepository;
    private readonly ILogger<DefaultPathLockStrategy> _logger;

    public DefaultPathLockStrategy(
        IRepository<TaskPathLock> lockRepository,
        IRepository<Agv> agvRepository,
        ILogger<DefaultPathLockStrategy> logger)
    {
        _lockRepository = lockRepository;
        _agvRepository = agvRepository;
        _logger = logger;
    }

    public Task<(bool Approved, string? Reason)> RequestLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode,
        Guid taskId)
    {
        // TODO: 实现通用的路径锁定算法
        _logger.LogWarning("使用默认路径锁定策略，暂未实现通用算法");
        return Task.FromResult((true, (string?)null));
    }

    public Task ReleaseLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode)
    {
        // TODO: 实现通用的路径释放算法
        _logger.LogWarning("使用默认路径释放策略，暂未实现通用算法");
        return Task.CompletedTask;
    }

    public Task ClearAgvLocksAsync(string agvCode)
    {
        // TODO: 实现通用的清理逻辑
        _logger.LogWarning("使用默认清理策略，暂未实现通用算法");
        return Task.CompletedTask;
    }
}
