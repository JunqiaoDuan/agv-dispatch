using System.Text.Json.Serialization;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 状态上报消息 (agv/{agvCode}/status)
/// 小车定时（每5秒）或状态变化时发布
/// </summary>
public class StatusMessage
{
    /// <summary>
    /// 小车编码
    /// </summary>
    [JsonPropertyName("agvCode")]
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// 小车状态
    /// </summary>
    [JsonPropertyName("status")]
    public AgvStatus Status { get; set; }

    /// <summary>
    /// 电量百分比 (0-100)
    /// </summary>
    [JsonPropertyName("battery")]
    public int Battery { get; set; }

    /// <summary>
    /// 电池电压真实值 (V)
    /// </summary>
    [JsonPropertyName("batteryVoltage")]
    public double BatteryVoltage { get; set; }

    /// <summary>
    /// 当前速度 (m/s)
    /// </summary>
    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    /// <summary>
    /// 位置信息
    /// </summary>
    [JsonPropertyName("position")]
    public PositionInfo Position { get; set; } = new();

    /// <summary>
    /// 当前任务ID，无任务时为null
    /// </summary>
    [JsonPropertyName("currentTaskId")]
    public string? CurrentTaskId { get; set; }

    /// <summary>
    /// 错误码，正常时为null
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 消息描述，正常时为null
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

}
