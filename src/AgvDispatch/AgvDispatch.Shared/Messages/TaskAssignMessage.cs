using System.Text.Json.Serialization;
using AgvDispatch.Shared.DTOs.Routes;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 任务下发消息 (agv/{agvCode}/task/assign)
/// 服务器向小车下发任务
/// </summary>
public class TaskAssignMessage
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    [JsonPropertyName("taskType")]
    public TaskJobType TaskType { get; set; }

    /// <summary>
    /// 优先级，数值越大优先级越低
    /// 默认 30；有效值： 10、20、30、40、50
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// 时间戳 (ISO 8601 格式)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// 起点 站点编号，如 S001
    /// </summary>
    [JsonPropertyName("startStationCode")]
    public string StartStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 终点 站点编号，如 S002
    /// </summary>
    [JsonPropertyName("endStationCode")]
    public string EndStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 路线检查点列表
    /// </summary>
    [JsonPropertyName("checkpoints")]
    public List<CheckpointDto>? Checkpoints { get; set; }
}
