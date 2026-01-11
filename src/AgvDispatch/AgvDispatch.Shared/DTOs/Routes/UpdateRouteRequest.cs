using System.ComponentModel.DataAnnotations;

namespace AgvDispatch.Shared.DTOs.Routes;

/// <summary>
/// 更新路线请求
/// </summary>
public class UpdateRouteRequest
{
    [Required(ErrorMessage = "路线名称不能为空")]
    [StringLength(100, ErrorMessage = "路线名称长度不能超过100个字符")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int SortNo { get; set; }

    public List<RouteSegmentRequest> Segments { get; set; } = [];
}
