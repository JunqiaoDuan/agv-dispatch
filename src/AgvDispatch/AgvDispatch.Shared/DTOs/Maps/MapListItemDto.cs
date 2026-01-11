namespace AgvDispatch.Shared.DTOs.Maps;

/// <summary>
/// 地图列表项 DTO
/// </summary>
public class MapListItemDto
{
    public Guid Id { get; set; }
    public string MapCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public bool IsActive { get; set; }
    public int SortNo { get; set; }
    public int NodeCount { get; set; }
    public int EdgeCount { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}
