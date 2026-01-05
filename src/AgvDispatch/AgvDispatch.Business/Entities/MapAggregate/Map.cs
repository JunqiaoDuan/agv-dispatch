using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.MapAggregate;

/// <summary>
/// 地图
/// </summary>
public class Map : BaseEntity
{
    /// <summary>
    /// 地图编号，如 M001
    /// </summary>
    public string MapCode { get; set; } = string.Empty;

    /// <summary>
    /// 地图名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 宽度 (厘米)
    /// </summary>
    public decimal Width { get; set; }

    /// <summary>
    /// 高度 (厘米)
    /// </summary>
    public decimal Height { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }
}
