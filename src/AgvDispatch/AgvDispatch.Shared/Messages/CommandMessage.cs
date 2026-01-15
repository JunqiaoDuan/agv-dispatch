using System.Text.Json.Serialization;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 控制指令消息 (agv/{agvCode}/command)
/// 服务器向小车下发控制指令
/// </summary>
public class CommandMessage
{
    /// <summary>
    /// 指令ID
    /// </summary>
    [JsonPropertyName("commandId")]
    public string CommandId { get; set; } = string.Empty;

    /// <summary>
    /// 指令类型
    /// </summary>
    [JsonPropertyName("commandType")]
    public CommandType CommandType { get; set; }

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// 指令参数
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, object>? Params { get; set; }
}
