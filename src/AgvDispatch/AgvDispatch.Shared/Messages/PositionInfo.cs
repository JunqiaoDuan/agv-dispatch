using System.Text.Json.Serialization;

namespace AgvDispatch.Shared.Messages;

/// <summary>
/// 位置信息
/// </summary>
public class PositionInfo
{
    /// <summary>
    /// 朝向角度 (0-360度，正东为0)
    /// </summary>
    [JsonPropertyName("angle")]
    public double? Angle { get; set; }

    /// <summary>
    /// X坐标 (厘米)
    /// </summary>
    [JsonPropertyName("x")]
    public double? X { get; set; }

    /// <summary>
    /// Y坐标 (厘米)
    /// </summary>
    [JsonPropertyName("y")]
    public double? Y { get; set; }

    /// <summary>
    /// 当前站点ID，不在站点时为null
    /// </summary>
    [JsonPropertyName("stationCode")]
    public string? StationCode { get; set; }

}
