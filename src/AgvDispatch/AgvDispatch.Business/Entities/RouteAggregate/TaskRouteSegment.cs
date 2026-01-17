using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.RouteAggregate;

/// <summary>
/// 任务路线段 (Web显示路径)
/// 用于Web端可视化渲染，关联地图节点和边
/// </summary>
public class TaskRouteSegment : BaseEntity
{
    /// <summary>
    /// 所属路线ID
    /// </summary>
    public Guid TaskRouteId { get; set; }

    /// <summary>
    /// 顺序号（10、20、30、40等）
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 引用的地图边ID
    /// </summary>
    public Guid MapEdgeId { get; set; }

    /// <summary>
    /// 行驶方向
    /// </summary>
    public DriveDirection Direction { get; set; } = DriveDirection.Forward;

    /// <summary>
    /// 到达后动作
    /// </summary>
    public FinalAction FinalAction { get; set; } = FinalAction.None;

}
