using AgvDispatch.Shared.Enums;
using TaskJobStatus = AgvDispatch.Shared.Enums.TaskJobStatus;

namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 任务详情 DTO
/// </summary>
public class TaskDetailDto
{
    public Guid Id { get; set; }
    public TaskJobType TaskType { get; set; }
    public TaskJobStatus TaskStatus { get; set; }
    public int Priority { get; set; }
    public string StartStationCode { get; set; } = string.Empty;
    public string EndStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 已分配的小车ID
    /// </summary>
    public Guid? AssignedAgvId { get; set; }
    public string? AssignedAgvCode { get; set; }
    public Guid? AssignedBy { get; set; }
    public string? AssignedByName { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }

    /// <summary>
    /// 进度百分比
    /// </summary>
    public decimal? ProgressPercentage { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 取消原因
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 时间字段
    /// </summary>
    public DateTimeOffset? CreationDate { get; set; }
    public string? CreatedByName { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
}
