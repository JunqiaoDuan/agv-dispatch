using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.MapEdges;

/// <summary>
/// 地图边列表项 DTO
/// </summary>
public class MapEdgeListItemDto
{
    public Guid Id { get; set; }
    public Guid MapId { get; set; }
    public string EdgeCode { get; set; } = string.Empty;
    public Guid StartNodeId { get; set; }
    public string StartNodeCode { get; set; } = string.Empty;
    public string StartNodeName { get; set; } = string.Empty;
    public Guid EndNodeId { get; set; }
    public string EndNodeCode { get; set; } = string.Empty;
    public string EndNodeName { get; set; } = string.Empty;
    public EdgeType EdgeType { get; set; }
    public bool IsBidirectional { get; set; }
    public decimal? ArcViaX { get; set; }
    public decimal? ArcViaY { get; set; }
    public decimal? Curvature { get; set; }
    public decimal Distance { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}
