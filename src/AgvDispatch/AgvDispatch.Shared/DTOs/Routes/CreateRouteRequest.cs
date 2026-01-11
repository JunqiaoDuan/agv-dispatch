using System.ComponentModel.DataAnnotations;

namespace AgvDispatch.Shared.DTOs.Routes;

/// <summary>
/// 创建路线请求
/// </summary>
public class CreateRouteRequest
{
    [Required(ErrorMessage = "所属地图ID不能为空")]
    public Guid MapId { get; set; }

    [Required(ErrorMessage = "路线编号不能为空")]
    [StringLength(20, ErrorMessage = "路线编号长度不能超过20个字符")]
    public string RouteCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "路线名称不能为空")]
    [StringLength(100, ErrorMessage = "路线名称长度不能超过100个字符")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public int SortNo { get; set; }

    public List<RouteSegmentRequest> Segments { get; set; } = [];
}
