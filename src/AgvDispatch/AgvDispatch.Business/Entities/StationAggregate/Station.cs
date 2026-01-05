using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.StationAggregate;

/// <summary>
/// 业务站点
/// </summary>
public class Station : BaseEntity
{
    /// <summary>
    /// 所属地图ID
    /// </summary>
    public Guid MapId { get; set; }

    /// <summary>
    /// 关联的地图节点ID
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// 站点编号，如 S001
    /// </summary>
    public string StationCode { get; set; } = string.Empty;

    /// <summary>
    /// 站点名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 站点类型
    /// </summary>
    public StationType StationType { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }
}
