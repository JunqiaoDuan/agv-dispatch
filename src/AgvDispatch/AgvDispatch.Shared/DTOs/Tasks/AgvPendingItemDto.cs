using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 待处理小车项 DTO
/// 用于下料、返回、充电等查询场景
/// </summary>
public class AgvPendingItemDto
{
    public Guid AgvId { get; set; }
    public string AgvCode { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Battery { get; set; }
    public bool HasCargo { get; set; }
    public AgvStatus AgvStatus { get; set; }
    public string? CurrentStationCode { get; set; }
    public string? CurrentStationName { get; set; }
    public StationType? CurrentStationType { get; set; }
}
