using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 路段解锁通知消息 (agv/{agvCode}/path/unlock)
/// AGV离开路段时发送
/// </summary>
public class PathUnlockMessage
{
    /// <summary>
    /// 小车编号
    /// </summary>
    [JsonPropertyName("agvCode")]
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 任务编号
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// 起始站点
    /// </summary>
    [JsonPropertyName("fromStationCode")]
    public string FromStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标站点
    /// </summary>
    [JsonPropertyName("toStationCode")]
    public string ToStationCode { get; set; } = string.Empty;
}
