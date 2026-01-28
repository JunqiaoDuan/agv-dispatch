using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.PathLocks;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services.PathLockStrategies;

/// <summary>
/// Haining 项目专用路径锁定策略
/// </summary>
/// <remarks>
/// 针对 Haining 项目的业务场景，使用手动配置的锁定规则
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

    #region 公共接口实现

    public async Task<(bool Approved, string? Reason)> RequestLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode,
        Guid taskId)
    {
        _logger.LogInformation(
            "Haining策略：AGV {AgvCode} 申请锁定 {From} → {To}，任务 {TaskId}",
            agvCode, fromStationCode, toStationCode, taskId);

        // 1. 获取AGV实体
        var agv = await _agvRepository.FirstOrDefaultAsync(new AgvByAgvCodeSpec(agvCode));
        if (agv == null)
        {
            return (false, $"AGV {agvCode} 不存在");
        }

        // 2. 检查该AGV是否已经锁定了此路段
        var existingLock = await _lockRepository.FirstOrDefaultAsync(
            new PathLockBySegmentAndAgvSpec(fromStationCode, toStationCode, agv.Id, PathLockStatus.Approved));

        if (existingLock != null)
        {
            _logger.LogDebug("AGV {AgvCode} 已锁定路段 {From} → {To}，直接批准", agvCode, fromStationCode, toStationCode);
            return (true, "之前已批准过");
        }

        #region 请求锁定 - 进出车间通道管控

        var workshopResult = await HandleWorkshopChannelLockAsync(fromStationCode, toStationCode, agv.Id, taskId);
        if (workshopResult.HasValue)
        {
            return workshopResult.Value;
        }

        #endregion

        #region 判断是否有需要释放的地方（不影响输出）

        #region 进出车间通道释放

        await HandleWorkshopChannelReleaseAsync(fromStationCode, agv.Id, agvCode);

        #endregion

        #endregion

        // 4. 其他路段：直接批准，不需要创建记录
        _logger.LogDebug("非管控路段 {From} → {To}，直接批准", fromStationCode, toStationCode);
        return (true, "非管控路段");

    }

    public async Task ClearAgvLocksAsync(string agvCode)
    {
        _logger.LogInformation("Haining策略：清理 AGV {AgvCode} 的所有锁定", agvCode);

        var agv = await _agvRepository.FirstOrDefaultAsync(new AgvByAgvCodeSpec(agvCode));
        if (agv == null)
        {
            _logger.LogWarning("AGV {AgvCode} 不存在，无法清理锁定", agvCode);
            return;
        }

        var activeLocks = await _lockRepository.ListAsync(
            new PathLockByAgvAndStatusSpec(agv.Id, PathLockStatus.Approved));

        foreach (var lockRecord in activeLocks)
        {
            lockRecord.Status = PathLockStatus.Released;
            lockRecord.ReleasedTime = DateTimeOffset.UtcNow;
        }
        await _lockRepository.UpdateRangeAsync(activeLocks);
        await _lockRepository.SaveChangesAsync();
        _logger.LogInformation("清理完成：AGV {AgvCode} 共释放 {Count} 条锁定", agvCode, activeLocks.Count);
    }

    #endregion

    #region 进出车间通道管控

    /// <summary>
    /// 进厂方向的关键路段：S411 → S410 → S401 → S402 → S403
    /// </summary>
    private static readonly List<(string From, string To)> WorkshopEnteringSegments =
    [
        ("S411", "S410"),
        ("S410", "S401"),
        ("S401", "S402"),
        ("S402", "S403"),
    ];

    /// <summary>
    /// 出厂方向的关键路段：S403 → S402 → S401 → S410
    /// </summary>
    private static readonly List<(string From, string To)> WorkshopExitingSegments =
    [
        ("S203", "S403"),
        ("S202", "S402"),
        ("S201", "S401"),

        ("S403", "S402"),
        ("S402", "S401"),
        ("S401", "S410"),
        ("S410", "S412")
    ];

    /// <summary>
    /// 进厂释放点：S201/S202/S203
    /// </summary>
    private static readonly HashSet<string> WorkshopEnteringReleasePoints = ["S201", "S202", "S203"];

    /// <summary>
    /// 出厂释放点：S412
    /// </summary>
    private static readonly HashSet<string> WorkshopExitingReleasePoints = ["S412"];

    /// <summary>
    /// 处理进出车间通道锁定请求
    /// </summary>
    /// <returns>如果是车间通道路段，返回处理结果；否则返回 null</returns>
    private async Task<(bool Approved, string? Reason)?> HandleWorkshopChannelLockAsync(
        string fromStationCode,
        string toStationCode,
        Guid agvId,
        Guid taskId)
    {
        // 判断请求路段的方向类型
        var isEntering = WorkshopEnteringSegments.Contains((fromStationCode, toStationCode));
        var isExiting = WorkshopExitingSegments.Contains((fromStationCode, toStationCode));

        if (!isEntering && !isExiting)
        {
            return null; // 不是车间通道路段
        }

        // 确定当前方向和冲突方向
        List<(string From, string To)> currentSegments;
        List<(string From, string To)> conflictSegments;
        string direction;
        if (isEntering)
        {
            currentSegments = WorkshopEnteringSegments;
            conflictSegments = WorkshopExitingSegments;
            direction = "进厂";
        }
        else
        {
            currentSegments = WorkshopExitingSegments;
            conflictSegments = WorkshopEnteringSegments;
            direction = "出厂";
        }

        // 检查反方向是否有活跃的锁定
        // 查询所有已批准的路径锁
        var approvedLocks = await _lockRepository.ListAsync(
            new PathLockByStatusSpec(PathLockStatus.Approved));

        // 在内存中检查是否有冲突路段
        var hasConflict = approvedLocks.Any(x =>
            conflictSegments.Any(seg => seg.From == x.FromStationCode && seg.To == x.ToStationCode));

        if (hasConflict)
        {
            var reason = $"车间通道被反方向占用，当前请求：{direction}";
            _logger.LogWarning("拒绝锁定 {From} → {To}：{Reason}", fromStationCode, toStationCode, reason);

            return (false, reason);
        }

        // 无冲突，批准并锁定整条通道的所有路段
        var channelName = isEntering ? "进厂通道" : "出厂通道";
        await CreateBatchLockRecordsAsync(agvId, currentSegments, taskId, PathLockStatus.Approved, channelName);
        _logger.LogInformation("批准锁定整条{Direction}通道（共{Count}段），触发路段 {From} → {To}",
            direction, currentSegments.Count, fromStationCode, toStationCode);
        return (true, null);
    }

    /// <summary>
    /// 处理进出车间通道锁定释放
    /// </summary>
    private async Task HandleWorkshopChannelReleaseAsync(
        string fromStationCode, 
        // string toStationCode, 
        Guid agvId, 
        string agvCode)
    {
        // 判断是否是释放点
        bool shouldReleaseEntering = WorkshopEnteringReleasePoints.Contains(fromStationCode);
        bool shouldReleaseExiting = WorkshopExitingReleasePoints.Contains(fromStationCode);

        if (!shouldReleaseEntering && !shouldReleaseExiting)
        {
            _logger.LogDebug("站点 {To} 不是车间通道释放点", fromStationCode);
            return;
        }

        // 确定需要释放的路段
        List<(string From, string To)> segmentsToRelease;
        string direction;
        if (shouldReleaseEntering)
        {
            segmentsToRelease = WorkshopEnteringSegments;
            direction = "进厂";
        }
        else
        {
            segmentsToRelease = WorkshopExitingSegments;
            direction = "出厂";
        }

        _logger.LogInformation("AGV {AgvCode} 到达{Direction}释放点 {To}，释放通道锁定", agvCode, direction, fromStationCode);

        // 查询该AGV在这些路段的所有Approved锁定
        // 查询该AGV所有已批准的路径锁
        var agvApprovedLocks = await _lockRepository.ListAsync(
            new PathLockByAgvAndStatusSpec(agvId, PathLockStatus.Approved));

        // 在内存中过滤出要释放的路段
        var locksToRelease = agvApprovedLocks
            .Where(x => segmentsToRelease.Any(seg => seg.From == x.FromStationCode && seg.To == x.ToStationCode))
            .ToList();

        if (locksToRelease.Count == 0)
        {
            _logger.LogDebug("AGV {AgvCode} 没有需要释放的车间通道锁定", agvCode);
            return;
        }

        // 释放锁定
        foreach (var lockRecord in locksToRelease)
        {
            lockRecord.Status = PathLockStatus.Released;
            lockRecord.ReleasedTime = DateTimeOffset.UtcNow;
            await _lockRepository.UpdateAsync(lockRecord);

            _logger.LogDebug(
                "释放锁定：AGV {AgvCode}，路段 {From} → {To}",
                agvCode, lockRecord.FromStationCode, lockRecord.ToStationCode);
        }

        await _lockRepository.SaveChangesAsync();
        _logger.LogInformation("AGV {AgvCode} 共释放 {Count} 条车间通道锁定", agvCode, locksToRelease.Count);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 批量创建锁定记录
    /// </summary>
    private async Task CreateBatchLockRecordsAsync(
        Guid agvId,
        List<(string From, string To)> segments,
        Guid taskId,
        PathLockStatus status,
        string? channelName = null,
        string? reason = null)
    {
        var now = DateTimeOffset.UtcNow;
        var lockRecords = segments.Select(seg =>
        {
            var _item = new TaskPathLock
            {
                Id = NewId.NextSequentialGuid(),
                FromStationCode = seg.From,
                ToStationCode = seg.To,
                LockedByAgvId = agvId,
                TaskId = taskId,
                Status = status,
                ChannelName = channelName,
                RequestTime = now,
                ApprovedTime = status == PathLockStatus.Approved ? now : null,
                RejectedReason = reason
            };
            _item.OnCreate();
            return _item;

        }).ToList();

        await _lockRepository.AddRangeAsync(lockRecords);
        await _lockRepository.SaveChangesAsync();
    }

    #endregion
}
