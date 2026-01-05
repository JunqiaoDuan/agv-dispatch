using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.RouteAggregate;

/// <summary>
/// 路线段
/// </summary>
public class RouteSegment : BaseEntity
{
    /// <summary>
    /// 所属路线ID
    /// </summary>
    public Guid RouteId { get; set; }

    /// <summary>
    /// 引用的边ID
    /// </summary>
    public Guid EdgeId { get; set; }

    /// <summary>
    /// 顺序号（10、20、30、40等）
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 行驶方向
    /// </summary>
    public DriveDirection Direction { get; set; } = DriveDirection.Forward;

    /// <summary>
    /// 到达后动作
    /// </summary>
    public FinalAction Action { get; set; } = FinalAction.None;

    /// <summary>
    /// 等待时间（秒）
    /// </summary>
    public int WaitTime { get; set; } = 0;
}
