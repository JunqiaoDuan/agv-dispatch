using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.TaskAggregate;

/// <summary>
/// 任务实体
/// </summary>
public class TaskJob : BaseEntity
{
    /// <summary>
    /// 任务类型
    /// </summary>
    public TaskJobType TaskType { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskJobStatus TaskStatus { get; set; } = TaskJobStatus.Pending;

    /// <summary>
    /// 优先级,数值越大优先级越低(10最高,50最低,默认30)
    /// </summary>
    public int Priority { get; set; } = 30;

    /// <summary>
    /// 起点站点编号
    /// </summary>
    public string StartStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 终点站点编号
    /// </summary>
    public string EndStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 分配的小车ID
    /// </summary>
    public Guid? AssignedAgvId { get; set; }

    /// <summary>
    /// 进度百分比(0-100)
    /// </summary>
    public decimal? ProgressPercentage { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }

    #region 类审计字段

    /// <summary>
    /// 确认分配的工人ID(手动调度版新增)
    /// </summary>
    public Guid? AssignedBy { get; set; }

    /// <summary>
    /// 确认分配的工人姓名(手动调度版新增)
    /// </summary>
    public string? AssignedByName { get; set; }

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset? AssignedAt { get; set; }

    /// <summary>
    /// 开始执行时间
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// 取消时间
    /// </summary>
    public DateTimeOffset? CancelledAt { get; set; }

    /// <summary>
    /// 取消原因
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string? FailureReason { get; set; }

    #endregion

}
