using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.TaskRouteAggregate;

/// <summary>
/// 任务路线定义
/// </summary>
public class TaskRoute : BaseEntity
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 起始站点编号
    /// </summary>
    public string StartStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 终点站点编号
    /// </summary>
    public string EndStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }
}
