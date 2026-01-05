using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.MapAggregate;

/// <summary>
/// 地图边
/// </summary>
public class MapEdge : BaseEntity
{
    /// <summary>
    /// 所属地图ID
    /// </summary>
    public Guid MapId { get; set; }

    /// <summary>
    /// 边编号，如 E001
    /// </summary>
    public string EdgeCode { get; set; } = string.Empty;

    /// <summary>
    /// 起点节点ID
    /// </summary>
    public Guid StartNodeId { get; set; }

    /// <summary>
    /// 终点节点ID
    /// </summary>
    public Guid EndNodeId { get; set; }

    /// <summary>
    /// 边类型
    /// </summary>
    public EdgeType EdgeType { get; set; } = EdgeType.Line;

    /// <summary>
    /// 是否双向通行
    /// </summary>
    public bool IsBidirectional { get; set; } = true;

    /// <summary>
    /// 弧线经过点X（弧线时使用）
    /// </summary>
    public decimal? ArcViaX { get; set; }

    /// <summary>
    /// 弧线经过点Y（弧线时使用）
    /// </summary>
    public decimal? ArcViaY { get; set; }

    /// <summary>
    /// 边长度
    /// </summary>
    public decimal Distance { get; set; } = 0;
}
