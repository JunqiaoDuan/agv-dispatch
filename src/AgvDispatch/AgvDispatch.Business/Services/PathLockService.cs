using AgvDispatch.Business.Services.PathLockStrategies;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 路径锁定服务实现（使用策略模式支持不同项目的专用逻辑）
/// </summary>
public class PathLockService : IPathLockService
{
    private readonly PathLockStrategyFactory _strategyFactory;
    private readonly ILogger<PathLockService> _logger;
    private readonly IPathLockStrategy _strategy;

    public PathLockService(
        PathLockStrategyFactory strategyFactory,
        ILogger<PathLockService> logger)
    {
        _strategyFactory = strategyFactory;
        _logger = logger;
        _strategy = _strategyFactory.CreateStrategy();
    }

    public async Task<(bool Approved, string? Reason)> RequestLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode,
        Guid taskId)
    {
        _logger.LogDebug(
            "委托给策略 {StrategyType} 处理锁定申请",
            _strategy.GetType().Name);

        return await _strategy.RequestLockAsync(
            fromStationCode,
            toStationCode,
            agvCode,
            taskId);
    }

    public async Task ReleaseLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode)
    {
        _logger.LogDebug(
            "委托给策略 {StrategyType} 处理锁定释放",
            _strategy.GetType().Name);

        await _strategy.ReleaseLockAsync(
            fromStationCode,
            toStationCode,
            agvCode);
    }

    public async Task ClearAgvLocksAsync(string agvCode)
    {
        _logger.LogDebug(
            "委托给策略 {StrategyType} 处理锁定清理",
            _strategy.GetType().Name);

        await _strategy.ClearAgvLocksAsync(agvCode);
    }
}
