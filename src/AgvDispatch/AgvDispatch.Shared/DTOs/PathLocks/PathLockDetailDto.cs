using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.PathLocks;

/// <summary>
/// 路径锁定详情DTO
/// </summary>
public class PathLockDetailDto
{
    /// <summary>
    /// 锁定记录ID
    /// </summary>
    public Guid Id { get; set; }

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
    /// 占用的小车编号
    /// </summary>
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 关联任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskJobStatus TaskStatus { get; set; }

    /// <summary>
    /// 锁定状态
    /// </summary>
    public PathLockStatus Status { get; set; }

    /// <summary>
    /// 通道名称
    /// </summary>
    public string? ChannelName { get; set; }

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTimeOffset RequestTime { get; set; }

    /// <summary>
    /// 批准时间
    /// </summary>
    public DateTimeOffset? ApprovedTime { get; set; }
}
