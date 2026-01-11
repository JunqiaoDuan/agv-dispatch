using System.ComponentModel.DataAnnotations;

namespace AgvDispatch.Shared.DTOs.MapNodes;

/// <summary>
/// 更新节点请求
/// </summary>
public class UpdateMapNodeRequest
{
    [Required(ErrorMessage = "节点名称不能为空")]
    [StringLength(100, ErrorMessage = "节点名称长度不能超过100个字符")]
    public string DisplayName { get; set; } = string.Empty;

    public decimal X { get; set; }

    public decimal Y { get; set; }

    [StringLength(200, ErrorMessage = "备注长度不能超过200个字符")]
    public string? Remark { get; set; }

    public int SortNo { get; set; }
}
