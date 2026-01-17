namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 取消任务请求 DTO
/// </summary>
public class CancelTaskRequestDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 取消原因
    /// </summary>
    public string? Reason { get; set; }
}
