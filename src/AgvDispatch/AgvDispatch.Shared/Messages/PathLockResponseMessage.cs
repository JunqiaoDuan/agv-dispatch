using AgvDispatch.Shared.Enums;
using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 路段锁定响应消息 (agv/{agvCode}/path/lock-response)
/// 服务器收到锁定请求后立即响应
/// </summary>
public class PathLockResponseMessage
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

    /// <summary>
    /// 响应状态
    /// </summary>
    [JsonPropertyName("status")]
    public PathLockStatus Status { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
