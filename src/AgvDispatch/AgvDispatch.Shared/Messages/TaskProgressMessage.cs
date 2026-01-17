using AgvDispatch.Shared.Enums;
using System;
using System.Text.Json.Serialization;
using TaskJobStatus = AgvDispatch.Shared.Enums.TaskJobStatus;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 任务进度消息 (agv/{agvCode}/task/progress)
/// 小车上报任务执行进度
/// </summary>
public class TaskProgressMessage
{
    /// <summary>
    /// 小车ID
    /// </summary>
    [JsonPropertyName("agvCode")]
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// 进度百分比 (0-100)
    /// </summary>
    [JsonPropertyName("progressPercentage")]
    public double? ProgressPercentage { get; set; } = 0;

    /// <summary>
    /// 任务状态
    /// </summary>
    [JsonPropertyName("status")]
    public TaskJobStatus Status { get; set; }

    /// <summary>
    /// 进度消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
