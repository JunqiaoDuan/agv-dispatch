using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.RouteAggregate;

/// <summary>
/// 路线
/// </summary>
public class Route : BaseEntity
{
    /// <summary>
    /// 所属地图ID
    /// </summary>
    public Guid MapId { get; set; }

    /// <summary>
    /// 路线编号，如 R001
    /// </summary>
    public string RouteCode { get; set; } = string.Empty;

    /// <summary>
    /// 路线名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }
}
