using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Routes;

/// <summary>
/// 路线段 DTO
/// </summary>
public class RouteSegmentDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid EdgeId { get; set; }
    public string EdgeCode { get; set; } = string.Empty;
    public string StartNodeCode { get; set; } = string.Empty;
    public string StartNodeName { get; set; } = string.Empty;
    public string EndNodeCode { get; set; } = string.Empty;
    public string EndNodeName { get; set; } = string.Empty;
    public int Seq { get; set; }
    public DriveDirection Direction { get; set; }
    public FinalAction Action { get; set; }
    public int WaitTime { get; set; }
}
