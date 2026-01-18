namespace AgvDispatch.Shared.DTOs.PathSegments;

/// <summary>
/// 路段锁定配置列表项DTO
/// </summary>
public class PathSegmentLockConfigListItemDto
{
    public Guid Id { get; set; }
    public string FromStationCode { get; set; } = string.Empty;
    public string ToStationCode { get; set; } = string.Empty;
    public bool IsLockRequired { get; set; }
    public string? LockReason { get; set; }
    public int TimeoutMinutes { get; set; }
    public int? Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public string? CreatedByName { get; set; }
}
