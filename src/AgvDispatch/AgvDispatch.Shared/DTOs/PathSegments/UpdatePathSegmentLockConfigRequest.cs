using System.ComponentModel.DataAnnotations;

namespace AgvDispatch.Shared.DTOs.PathSegments;

/// <summary>
/// 更新路段锁定配置请求DTO
/// </summary>
public class UpdatePathSegmentLockConfigRequest
{
    /// <summary>
    /// 是否需要锁定
    /// </summary>
    public bool IsLockRequired { get; set; }

    /// <summary>
    /// 锁定理由描述
    /// </summary>
    public string? LockReason { get; set; }

    /// <summary>
    /// 超时时间(分钟)
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
    public bool IsActive { get; set; }
}
