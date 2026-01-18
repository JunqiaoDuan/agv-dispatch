using System.ComponentModel.DataAnnotations;

namespace AgvDispatch.Shared.DTOs.PathSegments;

/// <summary>
/// 创建路段锁定配置请求DTO
/// </summary>
public class CreatePathSegmentLockConfigRequest
{
    /// <summary>
    /// 起始站点编号
    /// </summary>
    [Required(ErrorMessage = "起始站点编号不能为空")]
    public string FromStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标站点编号
    /// </summary>
    [Required(ErrorMessage = "目标站点编号不能为空")]
    public string ToStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否需要锁定
    /// </summary>
    public bool IsLockRequired { get; set; } = true;

    /// <summary>
    /// 锁定理由描述
    /// </summary>
    public string? LockReason { get; set; }

    /// <summary>
    /// 超时时间(分钟),默认10分钟
    /// </summary>
    [Range(1, 60, ErrorMessage = "超时时间必须在1-60分钟之间")]
    public int TimeoutMinutes { get; set; } = 10;

    /// <summary>
    /// 优先级(可选)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

}
