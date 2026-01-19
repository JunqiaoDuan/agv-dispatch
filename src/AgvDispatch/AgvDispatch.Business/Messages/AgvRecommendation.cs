using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Messages;

/// <summary>
/// 小车推荐结果
/// </summary>
public class AgvRecommendation
{
    public Guid AgvId { get; set; }
    public string AgvCode { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public double TotalScore { get; set; }
    public double BatteryScore { get; set; }
    public double StatusScore { get; set; }
    public double StationPriorityScore { get; set; }
    public int Battery { get; set; }
    public AgvStatus Status { get; set; }
    public bool HasCargo { get; set; }
    public string? CurrentStationCode { get; set; }
    public int? CurrentStationPriority { get; set; }
    public string RecommendReason { get; set; } = string.Empty;
}
