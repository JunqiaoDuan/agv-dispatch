using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.TaskAggregate;

/// <summary>
/// 任务进度日志实体
/// 设计原则：存储MQTT消息原样，不冗余存储需要查询才能得到的字段
/// </summary>
public class TaskProgressLog : BaseEntity
{
    // ==================== MQTT消息原样字段 ====================

    /// <summary>
    /// 任务ID (来自MQTT消息，字符串格式)
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 小车编号 (来自MQTT消息)
    /// </summary>
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskJobStatus Status { get; set; }

    /// <summary>
    /// 进度百分比 (0-100)
    /// </summary>
    public double? ProgressPercentage { get; set; }

    /// <summary>
    /// 进度消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 上报时间
    /// </summary>
    public DateTimeOffset ReportTime { get; set; }
}
