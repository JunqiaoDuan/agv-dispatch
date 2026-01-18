using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Stations;

/// <summary>
/// 站点列表项 DTO
/// </summary>
public class StationListItemDto
{
    public Guid Id { get; set; }
    public Guid MapId { get; set; }
    public Guid NodeId { get; set; }
    public string StationCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public StationType StationType { get; set; }
    public string? Description { get; set; }
    public int SortNo { get; set; }

    /// <summary>
    /// 优先级 (0-100, 数值越大优先级越高)
    /// </summary>
    public int Priority { get; set; } = 50;

    /// <summary>
    /// 关联的节点编号
    /// </summary>
    public string NodeCode { get; set; } = string.Empty;

    /// <summary>
    /// 关联的节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 节点 X 坐标
    /// </summary>
    public decimal X { get; set; }

    /// <summary>
    /// 节点 Y 坐标
    /// </summary>
    public decimal Y { get; set; }

    public DateTimeOffset? CreationDate { get; set; }
}
