using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.DTOs.BackgroundJobLogs;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.BackgroundJobLogAggregate;

/// <summary>
/// 后台任务执行日志实体
/// </summary>
public class BackgroundJobLog : BaseEntity
{
    /// <summary>
    /// 任务名称（如 AgvHealthCheckJob）
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务显示名称（如 AGV健康检测）
    /// </summary>
    public string JobDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 执行时间
    /// </summary>
    public DateTimeOffset ExecuteTime { get; set; }

    /// <summary>
    /// 执行结果（Success/Failed）
    /// </summary>
    public JobExecutionResult Result { get; set; }

    /// <summary>
    /// 执行内容描述（如：标记 2 台小车为离线）
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 详细信息（JSON格式，记录具体操作的数据）
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// 影响的实体数量
    /// </summary>
    public int AffectedCount { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// 错误信息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
}
