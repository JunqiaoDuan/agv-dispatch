using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.MapAggregate;

/// <summary>
/// 地图节点
/// </summary>
public class MapNode : BaseEntity
{
    /// <summary>
    /// 所属地图ID
    /// </summary>
    public Guid MapId { get; set; }

    /// <summary>
    /// 节点编号，如 N001
    /// </summary>
    public string NodeCode { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// X坐标 (厘米)
    /// </summary>
    public decimal X { get; set; }

    /// <summary>
    /// Y坐标 (厘米)
    /// </summary>
    public decimal Y { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }
}
