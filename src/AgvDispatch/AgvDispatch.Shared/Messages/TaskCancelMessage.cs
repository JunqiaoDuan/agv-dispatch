using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 取消任务消息 (agv/{agvCode}/task/cancel)
/// 服务器通知小车取消任务
/// </summary>
public class TaskCancelMessage
{
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
    /// 取消原因
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
