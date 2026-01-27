using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.TaskPathLockAggregate;

/// <summary>
/// 路径锁定实体
/// 记录路段占用情况，防止路径冲突
/// </summary>
public class TaskPathLock : BaseEntity
{
    /// <summary>
    /// 起始站点编号
    /// </summary>
    public string FromStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标站点编号
    /// </summary>
    public string ToStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 占用的小车ID
    /// </summary>
    public Guid LockedByAgvId { get; set; }

    /// <summary>
    /// 关联任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 锁定状态
    /// </summary>
    public PathLockStatus Status { get; set; } = PathLockStatus.Pending;

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTimeOffset RequestTime { get; set; }

    /// <summary>
    /// 批准时间
    /// </summary>
    public DateTimeOffset? ApprovedTime { get; set; }

    /// <summary>
    /// 拒绝原因
    /// </summary>
    public string? RejectedReason { get; set; }

    /// <summary>
    /// 释放时间
    /// </summary>
    public DateTimeOffset? ReleasedTime { get; set; }

    /// <summary>
    /// 过期时间（预留字段，当前不使用）
    /// </summary>
    public DateTimeOffset? ExpireAt { get; set; }
}
