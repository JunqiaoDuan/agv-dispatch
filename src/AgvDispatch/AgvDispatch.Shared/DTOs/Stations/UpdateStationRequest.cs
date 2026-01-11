using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Stations;

/// <summary>
/// 更新站点请求
/// </summary>
public class UpdateStationRequest
{
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
