using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Stations;

/// <summary>
/// 创建站点请求
/// </summary>
public class CreateStationRequest
{
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

    /// <summary>
    /// 优先级 (0-100, 数值越大优先级越高)
    /// </summary>
    public int Priority { get; set; } = 50;
}
