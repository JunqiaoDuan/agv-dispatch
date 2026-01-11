using System.ComponentModel.DataAnnotations;

namespace AgvDispatch.Shared.DTOs.Maps;

/// <summary>
/// 创建地图请求
/// </summary>
public class CreateMapRequest
{
    [Required(ErrorMessage = "地图编号不能为空")]
    [StringLength(20, ErrorMessage = "地图编号长度不能超过20个字符")]
    public string MapCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "地图名称不能为空")]
    [StringLength(100, ErrorMessage = "地图名称长度不能超过100个字符")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    [Range(1, 1000000, ErrorMessage = "宽度必须在1到1000000之间")]
    public decimal Width { get; set; }

    [Range(1, 1000000, ErrorMessage = "高度必须在1到1000000之间")]
    public decimal Height { get; set; }

    public int SortNo { get; set; }
}
