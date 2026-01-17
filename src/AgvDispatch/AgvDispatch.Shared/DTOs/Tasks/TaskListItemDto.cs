using AgvDispatch.Shared.Enums;
using TaskJobStatus = AgvDispatch.Shared.Enums.TaskJobStatus;

namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 任务列表项 DTO
/// </summary>
public class TaskListItemDto
{
    public Guid Id { get; set; }
    public TaskJobType TaskType { get; set; }
    public TaskJobStatus TaskStatus { get; set; }
    public int Priority { get; set; }
    public string StartStationCode { get; set; } = string.Empty;
    public string EndStationCode { get; set; } = string.Empty;
    public string? AssignedAgvCode { get; set; }
    public decimal? ProgressPercentage { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
