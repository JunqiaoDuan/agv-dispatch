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

    #region 双向互斥通道配置

    /// <summary>
    /// 双向互斥通道配置
    /// </summary>
    private class BidirectionalChannelConfig
    {
        /// <summary>
        /// 通道名称
        /// </summary>
        public required string ChannelName { get; init; }

        /// <summary>
        /// 方向A的路径段列表
        /// 需要请求锁定的路段
        /// </summary>
        public required List<(string From, string To)> DirectionASegments { get; init; }

        /// <summary>
        /// 方向B的路径段列表
        /// 需要请求锁定的路段
        /// </summary>
        public required List<(string From, string To)> DirectionBSegments { get; init; }

        /// <summary>
        /// 方向A的释放点
        /// </summary>
        public required HashSet<string> DirectionAReleasePoints { get; init; }

        /// <summary>
        /// 方向B的释放点
        /// </summary>
        public required HashSet<string> DirectionBReleasePoints { get; init; }

        /// <summary>
        /// 方向A的名称（用于日志）
        /// </summary>
        public required string DirectionAName { get; init; }

        /// <summary>
        /// 方向B的名称（用于日志）
        /// </summary>
        public required string DirectionBName { get; init; }

        /// <summary>
        /// 并发锁，防止同一通道的两个方向同时获得批准
        /// </summary>
        public SemaphoreSlim Lock { get; } = new(1, 1);
    }

    /// <summary>
    /// 进出车间通道配置
    /// </summary>
    private static readonly BidirectionalChannelConfig WorkshopChannelConfig = new()
    {
        ChannelName = "进出车间通道",
        DirectionASegments =
        [
            ("S411", "S410"),
            ("S410", "S401"),
            ("S401", "S402"),
            ("S402", "S403"),
        ],
        DirectionBSegments =
        [
            ("S203", "S403"),
            ("S202", "S402"),
            ("S201", "S401"),

            ("S403", "S402"),
            ("S402", "S401"),
            ("S401", "S410"),
            ("S410", "S412")
        ],
        DirectionAReleasePoints = ["S201", "S202", "S203"],
        DirectionBReleasePoints = ["S412"],
        DirectionAName = "进厂",
        DirectionBName = "出厂"
    };

    /// <summary>
    /// 西侧窄路配置
    /// </summary>
    private static readonly BidirectionalChannelConfig WestNarrowChannelConfig = new()
    {
        ChannelName = "西侧窄路",
        DirectionASegments =
        [
            ("S420", "S421"),
            ("S420", "S422"),
        ],
        DirectionBSegments =
        [
            ("S425", "S427"),
            ("S426", "S427"),
        ],
        DirectionAReleasePoints = ["S421", "S422"],
        DirectionBReleasePoints = ["S427"],
        DirectionAName = "去上料",
        DirectionBName = "去下料"
    };

    /// <summary>
    /// 所有双向互斥通道配置列表
    /// </summary>
    private static readonly List<BidirectionalChannelConfig> AllChannelConfigs =
    [
        WorkshopChannelConfig,
        WestNarrowChannelConfig
    ];

    #endregion

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

        #region 请求锁定 - 双向互斥通道管控

        // 遍历所有双向互斥通道配置，检查是否需要管控
        foreach (var channelConfig in AllChannelConfigs)
        {
            var result = await HandleBidirectionalChannelLockAsync(fromStationCode, toStationCode, agv.Id, taskId, channelConfig);
            if (result.HasValue)
            {
                return result.Value;
            }
        }

        #endregion

        #region 判断是否有需要释放的地方（不影响函数输出）

        // 遍历所有双向互斥通道配置，检查是否需要释放
        foreach (var channelConfig in AllChannelConfigs)
        {
            await HandleBidirectionalChannelReleaseAsync(fromStationCode, agv.Id, agvCode, channelConfig);
        }

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

    #region 双向互斥通道通用逻辑

    /// <summary>
    /// 处理双向互斥通道的锁定请求（通用方法）
    /// </summary>
    /// <param name="fromStationCode">起始站点</param>
    /// <param name="toStationCode">目标站点</param>
    /// <param name="agvId">AGV ID</param>
    /// <param name="taskId">任务 ID</param>
    /// <param name="config">通道配置</param>
    /// <returns>如果是该通道路段，返回处理结果；否则返回 null</returns>
    private async Task<(bool Approved, string? Reason)?> HandleBidirectionalChannelLockAsync(
        string fromStationCode,
        string toStationCode,
        Guid agvId,
        Guid taskId,
        BidirectionalChannelConfig config)
    {
        // 判断请求路段的方向类型
        var isDirectionA = config.DirectionASegments.Contains((fromStationCode, toStationCode));
        var isDirectionB = config.DirectionBSegments.Contains((fromStationCode, toStationCode));

        if (!isDirectionA && !isDirectionB)
        {
            return null; // 不是该通道路段
        }

        // 确定当前方向和冲突方向
        List<(string From, string To)> currentSegments;
        List<(string From, string To)> conflictSegments;
        string direction;
        string channelDisplayName;

        if (isDirectionA)
        {
            currentSegments = config.DirectionASegments;
            conflictSegments = config.DirectionBSegments;
            direction = config.DirectionAName;
            channelDisplayName = $"{config.ChannelName}-{config.DirectionAName}";
        }
        else
        {
            currentSegments = config.DirectionBSegments;
            conflictSegments = config.DirectionASegments;
            direction = config.DirectionBName;
            channelDisplayName = $"{config.ChannelName}-{config.DirectionBName}";
        }

        // 使用异步锁保护临界区，防止并发竞态条件
        await config.Lock.WaitAsync();
        try
        {
            // 检查反方向是否有活跃的锁定
            var approvedLocks = await _lockRepository.ListAsync(
                new PathLockByStatusSpec(PathLockStatus.Approved));

            // 在内存中检查是否有冲突路段
            var hasConflict = approvedLocks.Any(x =>
                conflictSegments.Any(seg => seg.From == x.FromStationCode && seg.To == x.ToStationCode));

            if (hasConflict)
            {
                var reason = $"{config.ChannelName}被反方向占用，当前请求：{direction}";
                _logger.LogWarning("拒绝锁定 {From} → {To}：{Reason}", fromStationCode, toStationCode, reason);

                return (false, reason);
            }

            // 无冲突，批准并锁定整条通道的所有路段
            await CreateBatchLockRecordsAsync(agvId, currentSegments, taskId, PathLockStatus.Approved, channelDisplayName);
            _logger.LogInformation("批准锁定整条{ChannelName}{Direction}通道（共{Count}段），触发路段 {From} → {To}",
                config.ChannelName, direction, currentSegments.Count, fromStationCode, toStationCode);
            return (true, null);
        }
        finally
        {
            config.Lock.Release();
        }
    }

    /// <summary>
    /// 处理双向互斥通道的锁定释放（通用方法）
    /// </summary>
    /// <param name="fromStationCode">当前站点</param>
    /// <param name="agvId">AGV ID</param>
    /// <param name="agvCode">AGV 编码</param>
    /// <param name="config">通道配置</param>
    private async Task HandleBidirectionalChannelReleaseAsync(
        string fromStationCode,
        Guid agvId,
        string agvCode,
        BidirectionalChannelConfig config)
    {
        // 判断是否是释放点
        bool shouldReleaseDirectionA = config.DirectionAReleasePoints.Contains(fromStationCode);
        bool shouldReleaseDirectionB = config.DirectionBReleasePoints.Contains(fromStationCode);

        if (!shouldReleaseDirectionA && !shouldReleaseDirectionB)
        {
            return; // 不是该通道的释放点
        }

        // 确定需要释放的路段
        List<(string From, string To)> segmentsToRelease;
        string direction;
        if (shouldReleaseDirectionA)
        {
            segmentsToRelease = config.DirectionASegments;
            direction = config.DirectionAName;
        }
        else
        {
            segmentsToRelease = config.DirectionBSegments;
            direction = config.DirectionBName;
        }

        _logger.LogInformation("AGV {AgvCode} 到达{ChannelName}{Direction}释放点 {From}，释放通道锁定",
            agvCode, config.ChannelName, direction, fromStationCode);

        // 查询该AGV所有已批准的路径锁
        var agvApprovedLocks = await _lockRepository.ListAsync(
            new PathLockByAgvAndStatusSpec(agvId, PathLockStatus.Approved));

        // 在内存中过滤出要释放的路段
        var locksToRelease = agvApprovedLocks
            .Where(x => segmentsToRelease.Any(seg => seg.From == x.FromStationCode && seg.To == x.ToStationCode))
            .ToList();

        if (locksToRelease.Count == 0)
        {
            _logger.LogDebug("AGV {AgvCode} 没有需要释放的{ChannelName}锁定", agvCode, config.ChannelName);
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
        _logger.LogInformation("AGV {AgvCode} 共释放 {Count} 条{ChannelName}锁定", agvCode, locksToRelease.Count, config.ChannelName);
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
