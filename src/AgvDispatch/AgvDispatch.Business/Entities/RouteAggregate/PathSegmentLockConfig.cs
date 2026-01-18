using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.RouteAggregate;

/// <summary>
/// 路段锁定配置实体
/// 用于配置哪些路段需要AGV提前申请占用权限
/// </summary>
public class PathSegmentLockConfig : BaseEntity
{
    /// <summary>
    /// 起始站点编号
    /// </summary>
    public string FromStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标站点编号
    /// </summary>
    public string ToStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否需要锁定
    /// </summary>
    public bool IsLockRequired { get; set; }

    /// <summary>
    /// 锁定理由描述
    /// </summary>
    public string? LockReason { get; set; }

    /// <summary>
    /// 超时时间(分钟),默认10分钟
    /// </summary>
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
