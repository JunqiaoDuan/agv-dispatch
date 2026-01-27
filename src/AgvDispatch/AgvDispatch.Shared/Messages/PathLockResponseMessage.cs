using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 路径锁定响应消息 (agv/{agvCode}/path/lock-response)
/// 服务端响应 AGV 的锁定请求
/// </summary>
public class PathLockResponseMessage
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 起始站点编号
    /// </summary>
    [JsonPropertyName("fromStation")]
    public string FromStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标站点编号
    /// </summary>
    [JsonPropertyName("toStation")]
    public string ToStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否批准
    /// </summary>
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    /// <summary>
    /// 拒绝原因
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
