using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.BackgroundJobLogs;

/// <summary>
/// 后台任务日志列表项 DTO
/// </summary>
public class BackgroundJobLogDto
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string JobDisplayName { get; set; } = string.Empty;
    public DateTimeOffset ExecuteTime { get; set; }
    public JobExecutionResult Result { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public int AffectedCount { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}
