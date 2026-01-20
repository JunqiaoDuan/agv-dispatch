using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Agvs;

/// <summary>
/// AGV异常摘要信息 DTO
/// </summary>
public class AgvExceptionSummaryDto
{
    public Guid Id { get; set; }
    public AgvExceptionType ExceptionType { get; set; }
    public AgvExceptionSeverity Severity { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset ExceptionTime { get; set; }
    public string? StationCode { get; set; }
}
