namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 分配小车请求 DTO
/// </summary>
public class AssignAgvRequestDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 选中的小车ID
    /// </summary>
    public Guid AgvId { get; set; }
}
