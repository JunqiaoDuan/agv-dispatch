using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Stations;

namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 地图渲染数据模型
/// </summary>
public class MapRenderData
{
    #region 通用 - 地图基础数据

    /// <summary>
    /// 地图宽度（厘米）
    /// </summary>
    public decimal Width { get; set; }

    /// <summary>
    /// 地图高度（厘米）
    /// </summary>
    public decimal Height { get; set; }

    /// <summary>
    /// 节点列表
    /// </summary>
    public List<MapNodeListItemDto> Nodes { get; set; } = [];

    /// <summary>
    /// 边列表
    /// </summary>
    public List<MapEdgeListItemDto> Edges { get; set; } = [];

    /// <summary>
    /// 站点列表
    /// </summary>
    public List<StationListItemDto> Stations { get; set; } = [];

    #endregion

    #region 编辑专用 - 选中状态

    /// <summary>
    /// 选中的节点ID
    /// </summary>
    public Guid? SelectedNodeId { get; set; }

    /// <summary>
    /// 选中的边ID
    /// </summary>
    public Guid? SelectedEdgeId { get; set; }

    /// <summary>
    /// 选中的站点ID
    /// </summary>
    public Guid? SelectedStationId { get; set; }

    #endregion

    #region 运行专用 - 动态状态

    /// <summary>
    /// 路线高亮的边ID列表（按顺序）
    /// </summary>
    public List<Guid> HighlightedEdgeIds { get; set; } = [];

    #endregion
}
