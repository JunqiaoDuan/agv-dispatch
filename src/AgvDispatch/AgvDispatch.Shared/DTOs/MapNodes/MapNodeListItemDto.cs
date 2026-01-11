namespace AgvDispatch.Shared.DTOs.MapNodes;

/// <summary>
/// 地图节点列表项 DTO
/// </summary>
public class MapNodeListItemDto
{
    public Guid Id { get; set; }
    public Guid MapId { get; set; }
    public string NodeCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public string? Remark { get; set; }
    public int SortNo { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}
