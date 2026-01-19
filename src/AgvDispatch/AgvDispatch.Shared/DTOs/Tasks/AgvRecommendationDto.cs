using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 小车推荐 DTO
/// </summary>
public class AgvRecommendationDto
{
    public Guid AgvId { get; set; }
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否可用
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// 总分(0-100)
    /// </summary>
    public double TotalScore { get; set; }

    /// <summary>
    /// 距离分数(0-40)
    /// </summary>
    public double DistanceScore { get; set; }

    /// <summary>
    /// 电池分数(0-30)
    /// </summary>
    public double BatteryScore { get; set; }

    /// <summary>
    /// 状态分数(0-30)
    /// </summary>
    public double StatusScore { get; set; }

    /// <summary>
    /// 距离(cm)
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// 电池电量(%)
    /// </summary>
    public int Battery { get; set; }

    /// <summary>
    /// 小车状态
    /// </summary>
    public AgvStatus Status { get; set; }

    /// <summary>
    /// 是否有料
    /// </summary>
    public bool HasCargo { get; set; }

    /// <summary>
    /// 推荐理由
    /// </summary>
    public string RecommendReason { get; set; } = string.Empty;
}
