using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 路径锁定请求消息 (agv/{agvCode}/path/lock-request)
/// AGV 请求锁定路段
/// </summary>
public class PathLockRequestMessage
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
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
