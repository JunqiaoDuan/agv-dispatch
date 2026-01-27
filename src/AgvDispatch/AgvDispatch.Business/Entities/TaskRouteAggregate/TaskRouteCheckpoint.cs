using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.TaskRouteAggregate;

/// <summary>
/// 任务路线检查点实体(AGV对接路径)
/// 用于AGV导航,只包含关键站点
/// </summary>
public class TaskRouteCheckpoint : BaseEntity
{
    /// <summary>
    /// 路线ID
    /// </summary>
    public Guid TaskRouteId { get; set; }

    /// <summary>
    /// 序号(10, 20, 30, ...)
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 站点编号
    /// </summary>
    public string StationCode { get; set; } = string.Empty;

    /// <summary>
    /// 检查点类型
    /// </summary>
    public CheckpointType CheckpointType { get; set; }

    /// <summary>
    /// 是否需要锁定(申请通行权)
    /// 根据地图EdgeLock配置在生成TaskRoute时计算并存储
    /// </summary>
    //public bool IsLockRequired { get; set; }
}
