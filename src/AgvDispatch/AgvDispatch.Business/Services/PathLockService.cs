using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Business.Services.PathLockStrategies;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.PathLocks;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Shared.DTOs.PathLocks;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Repository;
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
    private readonly IRepository<TaskPathLock> _lockRepository;
    private readonly IRepository<Agv> _agvRepository;
    private readonly IRepository<TaskJob> _taskRepository;

    public PathLockService(
        PathLockStrategyFactory strategyFactory,
        IRepository<TaskPathLock> lockRepository,
        IRepository<Agv> agvRepository,
        IRepository<TaskJob> taskRepository,
        ILogger<PathLockService> logger)
    {
        _strategyFactory = strategyFactory;
        _lockRepository = lockRepository;
        _agvRepository = agvRepository;
        _taskRepository = taskRepository;
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

    public async Task ClearAgvLocksAsync(string agvCode)
    {
        _logger.LogDebug(
            "委托给策略 {StrategyType} 处理锁定清理",
            _strategy.GetType().Name);

        await _strategy.ClearAgvLocksAsync(agvCode);
    }

    public async Task<List<ActiveChannelDto>> GetActiveChannelsAsync()
    {
        // 查询所有 Approved 状态且有 ChannelName 的锁定记录
        var activeLocks = await _lockRepository.ListAsync(new Specifications.PathLocks.ActivePathLocksWithChannelSpec());

        // 按 ChannelName 分组统计
        var channels = activeLocks
            .Where(l => !string.IsNullOrEmpty(l.ChannelName))
            .GroupBy(l => l.ChannelName!)
            .Select(g => new ActiveChannelDto
            {
                ChannelName = g.Key,
                AgvCount = g.Select(x => x.LockedByAgvId).Distinct().Count()
            })
            .OrderBy(c => c.ChannelName)
            .ToList();

        return channels;
    }

    public async Task<ChannelDetailDto?> GetChannelDetailAsync(string channelName)
    {
        // 查询该通道下所有 Approved 状态的路径锁定
        var locks = await _lockRepository.ListAsync(new ActivePathLocksByChannelSpec(channelName));

        if (!locks.Any())
        {
            return null;
        }

        // 获取所有相关的 AGV
        var agvIds = locks.Select(l => l.LockedByAgvId).Distinct().ToList();
        var agvs = await _agvRepository.ListAsync(new AgvByIdsSpec(agvIds));
        var agvDict = agvs.ToDictionary(a => a.Id, a => a.AgvCode);

        // 获取所有相关的任务
        var taskIds = locks.Select(l => l.TaskId).Distinct().ToList();
        var tasks = await _taskRepository.ListAsync(new TaskByIdsSpec(taskIds));
        var taskDict = tasks.ToDictionary(t => t.Id, t => t.TaskStatus);

        // 构建详情DTO
        var pathLocks = locks.Select(l => new PathLockDetailDto
        {
            Id = l.Id,
            FromStationCode = l.FromStationCode,
            ToStationCode = l.ToStationCode,
            LockedByAgvId = l.LockedByAgvId,
            AgvCode = agvDict.GetValueOrDefault(l.LockedByAgvId, "未知"),
            TaskId = l.TaskId,
            TaskStatus = taskDict.GetValueOrDefault(l.TaskId, TaskJobStatus.Failed),
            Status = l.Status,
            ChannelName = l.ChannelName,
            RequestTime = l.RequestTime,
            ApprovedTime = l.ApprovedTime
        }).ToList();

        return new ChannelDetailDto
        {
            ChannelName = channelName,
            PathLocks = pathLocks,
            AgvCount = agvIds.Count
        };
    }

    public async Task<int> ReleaseChannelAsync(string channelName)
    {
        // 查询该通道下所有 Approved 状态的路径锁定
        var locks = await _lockRepository.ListAsync(new ActivePathLocksByChannelSpec(channelName));

        if (!locks.Any())
        {
            _logger.LogWarning("[PathLockService] 通道 {ChannelName} 没有活跃的锁定记录", channelName);
            return 0;
        }

        // 获取所有相关的任务
        var taskIds = locks.Select(l => l.TaskId).Distinct().ToList();
        var tasks = await _taskRepository.ListAsync(new TaskByIdsSpec(taskIds));

        // 筛选出无效任务的锁定记录
        var waitingTaskIds = tasks
            .Where(t => t.TaskStatus == TaskJobStatus.Cancelled
                        || t.TaskStatus == TaskJobStatus.Failed)
            .Select(t => t.Id)
            .ToHashSet();

        var locksToRelease = locks.Where(l => waitingTaskIds.Contains(l.TaskId)).ToList();

        if (!locksToRelease.Any())
        {
            _logger.LogInformation("[PathLockService] 通道 {ChannelName} 没有可释放的锁定记录(无已取消任务)", channelName);
            return 0;
        }

        // 释放锁定
        foreach (var lockRecord in locksToRelease)
        {
            lockRecord.Status = PathLockStatus.Released;
            lockRecord.ReleasedTime = DateTimeOffset.UtcNow;
            await _lockRepository.UpdateAsync(lockRecord);
        }

        _logger.LogInformation(
            "[PathLockService] 手动释放通道 {ChannelName} 的 {Count} 个锁定记录",
            channelName,
            locksToRelease.Count);

        return locksToRelease.Count;
    }
}
