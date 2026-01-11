namespace AgvDispatch.Shared.DTOs.Routes;

/// <summary>
/// 路线详情 DTO
/// </summary>
public class RouteDetailDto
{
    public Guid Id { get; set; }
    public Guid MapId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SortNo { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public List<RouteSegmentDto> Segments { get; set; } = [];
}
