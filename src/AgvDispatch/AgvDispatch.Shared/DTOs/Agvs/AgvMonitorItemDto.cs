namespace AgvDispatch.Shared.DTOs.Agvs;

/// <summary>
/// AGV监控列表项 DTO (扩展AgvListItemDto，添加监控专用字段)
/// </summary>
public class AgvMonitorItemDto : AgvListItemDto
{
    /// <summary>
    /// 未解决的异常数量
    /// </summary>
    public int UnresolvedExceptionCount { get; set; }

    /// <summary>
    /// 最新的未解决异常（最多30条）
    /// </summary>
    public List<AgvExceptionSummaryDto> LatestExceptions { get; set; } = new();
}
