using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Agvs;

/// <summary>
/// AGV异常分页请求
/// </summary>
public class PagedAgvExceptionRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 是否只查询未解决的异常（null表示全部）
    /// </summary>
    public bool? OnlyUnresolved { get; set; }

    /// <summary>
    /// 严重程度筛选
    /// </summary>
    public AgvExceptionSeverity? Severity { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
