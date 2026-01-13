using System.ComponentModel.DataAnnotations;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.MapEdges;

/// <summary>
/// 创建边请求
/// </summary>
public class CreateMapEdgeRequest
{
    [Required(ErrorMessage = "边编号不能为空")]
    [StringLength(20, ErrorMessage = "边编号长度不能超过20个字符")]
    public string EdgeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "起点节点ID不能为空")]
    public Guid StartNodeId { get; set; }

    [Required(ErrorMessage = "终点节点ID不能为空")]
    public Guid EndNodeId { get; set; }

    public EdgeType EdgeType { get; set; } = EdgeType.Line;

    public bool IsBidirectional { get; set; } = true;

    public decimal? ArcViaX { get; set; }

    public decimal? ArcViaY { get; set; }

    public decimal? Curvature { get; set; }
}
