using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.MapEdges;

/// <summary>
/// 更新边请求
/// </summary>
public class UpdateMapEdgeRequest
{
    public EdgeType EdgeType { get; set; } = EdgeType.Line;

    public bool IsBidirectional { get; set; } = true;

    public decimal? ArcViaX { get; set; }

    public decimal? ArcViaY { get; set; }

    public decimal? Curvature { get; set; }
}
