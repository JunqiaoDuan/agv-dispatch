using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 路段锁定请求消息 (agv/{agvCode}/path/lock-request)
/// AGV即将到达中间点时发送,申请通行权
/// </summary>
public class PathLockRequestMessage
{
    /// <summary>
    /// 小车编号
    /// </summary>
    [JsonPropertyName("agvCode")]
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 当前任务编号
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// 起始站点(当前站点)
    /// </summary>
    [JsonPropertyName("fromStationCode")]
    public string FromStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标站点(下一个站点)
    /// </summary>
    [JsonPropertyName("toStationCode")]
    public string ToStationCode { get; set; } = string.Empty;
}
