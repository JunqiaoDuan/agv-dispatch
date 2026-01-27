using AgvDispatch.Shared.Enums;
using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.DTOs.Routes;

/// <summary>
/// 检查点DTO（用于消息传递）
/// </summary>
public class CheckpointDto
{
    /// <summary>
    /// 序号
    /// </summary>
    [JsonPropertyName("seq")]
    public int Seq { get; set; }

    /// <summary>
    /// 站点编号
    /// </summary>
    [JsonPropertyName("station")]
    public string StationCode { get; set; } = string.Empty;

    /// <summary>
    /// 检查点类型
    /// </summary>
    [JsonPropertyName("type")]
    public CheckpointType CheckpointType { get; set; }

}
