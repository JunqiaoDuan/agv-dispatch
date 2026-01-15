using System.Text.Json.Serialization;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 异常上报消息 (agv/{agvCode}/exception)
/// 小车发生异常时上报
/// </summary>
public class ExceptionMessage
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
    /// 异常类型
    /// </summary>
    [JsonPropertyName("exceptionType")]
    public AgvExceptionType ExceptionType { get; set; }

    /// <summary>
    /// 严重级别
    /// </summary>
    [JsonPropertyName("severity")]
    public AgvExceptionSeverity Severity { get; set; }

    /// <summary>
    /// 异常消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 发生位置
    /// </summary>
    [JsonPropertyName("position")]
    public PositionInfo? Position { get; set; }

    /// <summary>
    /// 当前任务ID
    /// </summary>
    [JsonPropertyName("taskId")]
    public string? TaskId { get; set; }
}
