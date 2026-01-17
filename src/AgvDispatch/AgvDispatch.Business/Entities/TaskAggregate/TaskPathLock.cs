using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.TaskAggregate;

/// <summary>
/// 任务路径锁定实体
/// 用于记录路段占用情况,防止路径冲突
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
    /// 批准时间(状态为Approved时填充)
    /// </summary>
    public DateTimeOffset? ApprovedTime { get; set; }

    /// <summary>
    /// 拒绝原因(状态为Rejected时填充)
    /// </summary>
    public string? RejectedReason { get; set; }

    /// <summary>
    /// 过期时间(可选,防止死锁)
    /// </summary>
    public DateTimeOffset? ExpireAt { get; set; }
}
